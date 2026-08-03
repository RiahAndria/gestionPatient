using Npgsql;

namespace Patients.Services;

public partial class PaiementService
{
    public void ConfirmerPaiement(string numeroPaiement, string modePaiement)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE PAIEMENT
            SET STATUT = true, MODEPAIEMENT = @Mode, DATEPAIEMENT = now()
            WHERE NUMEROPAIEMENT = @NumeroPaiement;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Mode", modePaiement);
        cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
        cmd.ExecuteNonQuery();
    }

    // Envoie une relance (ecrit une NOTIFICATION liee au rendez-vous
    // d'origine) et retourne le nombre total de relances envoyees.
    public int EnvoyerRelance(string numeroPaiement)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = "SELECT NUMERORDV FROM PAIEMENT WHERE NUMEROPAIEMENT = @NumeroPaiement;";
        string numeroRdv;
        using (var cmdRdv = new NpgsqlCommand(queryRdv, conn))
        {
            cmdRdv.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
            numeroRdv = (string)(cmdRdv.ExecuteScalar() ?? throw new InvalidOperationException("Paiement introuvable."));
        }

        int prochainNumero;
        using (var cmdCompte = new NpgsqlCommand("SELECT COUNT(*) FROM NOTIFICATION WHERE NUMERORDV = @NumeroRdv;", conn))
        {
            cmdCompte.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            prochainNumero = (int)(long)cmdCompte.ExecuteScalar()! + 1;
        }

        string queryInsert = @"
            INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF)
            VALUES (@NumeroNotif, @NumeroRdv, @Texte);";

        using var cmdInsert = new NpgsqlCommand(queryInsert, conn);
        cmdInsert.Parameters.AddWithValue("NumeroNotif", $"NOTIF-{Guid.NewGuid().ToString()[..8].ToUpper()}");
        cmdInsert.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmdInsert.Parameters.AddWithValue("Texte", $"Relance de paiement n°{prochainNumero} : merci de régulariser votre facture liée à ce rendez-vous.");
        cmdInsert.ExecuteNonQuery();

        return prochainNumero;
    }

    // A lancer manuellement (bouton "Traiter les impayes"). Pour chaque
    // facture NORMALE en attente depuis plus de DELAI_ANNULATION_JOURS
    // jours ET ayant recu au moins NB_RELANCES_MAX relances, annule tout
    // rendez-vous FUTUR encore planifie pour ce patient - SAUF si ce
    // rendez-vous futur a deja un acompte verse.
    public List<string> TraiterImpayes()
    {
        var actions = new List<string>();
        var enAttente = ObtenirEnAttente();

        foreach (var facture in enAttente)
        {
            bool delaiDepasse = facture.JoursEcoules >= DELAI_ANNULATION_JOURS;
            bool relancesEpuisees = facture.NombreRelances >= NB_RELANCES_MAX;

            if (!delaiDepasse || !relancesEpuisees) continue;

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string queryPatient = @"
                SELECT r.ID FROM PAIEMENT pa
                INNER JOIN RENDEZ_VOUS r ON pa.NUMERORDV = r.NUMERORDV
                WHERE pa.NUMEROPAIEMENT = @NumeroPaiement;";

            string patientId;
            using (var cmdPatient = new NpgsqlCommand(queryPatient, conn))
            {
                cmdPatient.Parameters.AddWithValue("NumeroPaiement", facture.NumeroPaiement);
                patientId = (string)cmdPatient.ExecuteScalar()!;
            }

            string queryFuturs = @"
                SELECT NUMERORDV FROM RENDEZ_VOUS
                WHERE ID = @PatientId AND STATUT = 'PLANIFIE' AND DATEHEURERDV > now();";

            var rdvFuturs = new List<string>();
            using (var cmdFuturs = new NpgsqlCommand(queryFuturs, conn))
            {
                cmdFuturs.Parameters.AddWithValue("PatientId", patientId);
                using var reader = cmdFuturs.ExecuteReader();
                while (reader.Read()) rdvFuturs.Add(reader.GetString(0));
            }

            foreach (var numeroRdv in rdvFuturs)
            {
                bool aUnAcompte = ObtenirTotalAcomptes(conn, numeroRdv) > 0;

                if (aUnAcompte)
                {
                    actions.Add($"Rendez-vous {numeroRdv} ({facture.PatientNom}) NON annulé automatiquement : un acompte a déjà été versé dessus — vérification manuelle recommandée.");
                    continue;
                }

                string queryAnnuler = @"
                    UPDATE RENDEZ_VOUS
                    SET STATUT = 'ANNULE', MOTIFANNULATION = @Motif
                    WHERE NUMERORDV = @NumeroRdv;";

                using var cmdAnnuler = new NpgsqlCommand(queryAnnuler, conn);
                cmdAnnuler.Parameters.AddWithValue("Motif", $"Annulation automatique : facture {facture.NumeroPaiement} impayée depuis {facture.JoursEcoules} jours après {facture.NombreRelances} relance(s).");
                cmdAnnuler.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                cmdAnnuler.ExecuteNonQuery();

                actions.Add($"Rendez-vous {numeroRdv} ({facture.PatientNom}) annulé automatiquement pour impayé.");
            }

            if (rdvFuturs.Count == 0)
            {
                actions.Add($"Facture {facture.NumeroPaiement} ({facture.PatientNom}) toujours impayée après délai — aucun rendez-vous futur à annuler pour ce patient.");
            }
        }

        if (actions.Count == 0)
            actions.Add("Aucune action nécessaire : pas de facture en retard au-delà des seuils configurés.");

        return actions;
    }
}
