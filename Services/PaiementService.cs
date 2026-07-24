using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class PaiementService
{
    private readonly string _connectionString;

    // Seuils de la politique de relance/annulation - a valider en equipe,
    // faciles a ajuster ici en un seul endroit.
    private const int NB_RELANCES_MAX = 3;
    private const int DELAI_ANNULATION_JOURS = 7;

    public PaiementService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // ---------------------------------------------------------------
    // A appeler par le module Consultation (Sylvia) juste apres avoir
    // enregistre une CONSULTATION. Cree la "facture" en attente.
    // Pas de paiement en avance : cette methode exige un
    // numeroConsultation qui existe deja en base (contrainte FK native
    // s'en charge, mais on verifie avant pour un message clair).
    // ---------------------------------------------------------------
    public void CreerPaiementDu(string numeroConsultation, decimal montant, string modePaiementPropose = "Non précisé")
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryVerif = "SELECT COUNT(*) FROM CONSULTATION WHERE NUMEROCONSULTATION = @NumeroConsultation;";
        using (var cmdVerif = new NpgsqlCommand(queryVerif, conn))
        {
            cmdVerif.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
            var existe = (long)cmdVerif.ExecuteScalar()! > 0;
            if (!existe)
                throw new InvalidOperationException("Impossible de créer un paiement : la consultation n'existe pas encore (pas de paiement en avance).");
        }

        string numeroPaiement = $"PAI-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        string query = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMEROCONSULTATION, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroConsultation, now(), @Montant, @Mode, false);";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
        cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
        cmd.Parameters.AddWithValue("Montant", montant);
        cmd.Parameters.AddWithValue("Mode", modePaiementPropose);
        cmd.ExecuteNonQuery();
    }

    // Montant suggere = taux horaire du medecin qui a tenu la consultation
    // (hypothese simplificatrice : tarif forfaitaire par consultation,
    // faute d'une duree stockee quelque part dans le schema actuel).
    public decimal CalculerMontantSuggere(string numeroConsultation)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            SELECT me.TAUXHORAIRE
            FROM CONSULTATION c
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_HER_2
            WHERE c.NUMEROCONSULTATION = @NumeroConsultation;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
        var resultat = cmd.ExecuteScalar();
        return resultat is decimal montant ? montant : 0m;
    }

    // ---------------------------------------------------------------
    // Liste des factures en attente, avec patient + nombre de relances
    // deja envoyees (compte le nombre de NOTIFICATION liees au RDV
    // d'origine de la consultation).
    // ---------------------------------------------------------------
    public List<PaiementAffichage> ObtenirEnAttente()
    {
        var resultat = new List<PaiementAffichage>();

        string query = @"
            SELECT pa.NUMEROPAIEMENT, pa.NUMEROCONSULTATION, r.NUMERORDV,
                   pp.NOM, pp.PRENOM, pa.DATEPAIEMENT, pa.MONTANT, pa.MODEPAIEMENT,
                   (SELECT COUNT(*) FROM NOTIFICATION n WHERE n.NUMERORDV = r.NUMERORDV) AS NBRELANCES
            FROM PAIEMENT pa
            INNER JOIN CONSULTATION c ON pa.NUMEROCONSULTATION = c.NUMEROCONSULTATION
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            INNER JOIN PATIENT pat ON r.ID = pat.ID
            INNER JOIN PERSONNE pp ON pat.ID = pp.ID
            WHERE pa.STATUT = false
            ORDER BY pa.DATEPAIEMENT;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new PaiementAffichage
            {
                NumeroPaiement = reader.GetString(0),
                NumeroConsultation = reader.GetString(1),
                NumeroRdv = reader.GetString(2),
                PatientNom = $"{reader.GetString(3)} {reader.GetString(4)}",
                DateFacture = reader.GetDateTime(5),
                Montant = reader.GetDecimal(6),
                ModePaiement = reader.GetString(7),
                EstPaye = false,
                NombreRelances = (int)reader.GetInt64(8)
            });
        }

        return resultat;
    }

    // Historique des paiements deja regles.
    public List<PaiementAffichage> ObtenirHistoriquePayes()
    {
        var resultat = new List<PaiementAffichage>();

        string query = @"
            SELECT pa.NUMEROPAIEMENT, pa.NUMEROCONSULTATION, r.NUMERORDV,
                   pp.NOM, pp.PRENOM, pa.DATEPAIEMENT, pa.MONTANT, pa.MODEPAIEMENT
            FROM PAIEMENT pa
            INNER JOIN CONSULTATION c ON pa.NUMEROCONSULTATION = c.NUMEROCONSULTATION
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            INNER JOIN PATIENT pat ON r.ID = pat.ID
            INNER JOIN PERSONNE pp ON pat.ID = pp.ID
            WHERE pa.STATUT = true
            ORDER BY pa.DATEPAIEMENT DESC;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new PaiementAffichage
            {
                NumeroPaiement = reader.GetString(0),
                NumeroConsultation = reader.GetString(1),
                NumeroRdv = reader.GetString(2),
                PatientNom = $"{reader.GetString(3)} {reader.GetString(4)}",
                DateFacture = reader.GetDateTime(5),
                Montant = reader.GetDecimal(6),
                ModePaiement = reader.GetString(7),
                EstPaye = true
            });
        }

        return resultat;
    }

    // ---------------------------------------------------------------
    // Confirme qu'un paiement a bien ete recu.
    // ---------------------------------------------------------------
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

    // ---------------------------------------------------------------
    // Envoie une relance de paiement (ecrit une NOTIFICATION liee au
    // RDV d'origine). Retourne le nombre total de relances envoyees
    // pour ce paiement, relances comprises.
    // ---------------------------------------------------------------
    public int EnvoyerRelance(string numeroPaiement)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = @"
            SELECT r.NUMERORDV FROM PAIEMENT pa
            INNER JOIN CONSULTATION c ON pa.NUMEROCONSULTATION = c.NUMEROCONSULTATION
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            WHERE pa.NUMEROPAIEMENT = @NumeroPaiement;";

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

    // ---------------------------------------------------------------
    // A lancer manuellement (bouton "Traiter les impayes") : pour
    // chaque facture en attente depuis plus de DELAI_ANNULATION_JOURS
    // jours ET ayant deja recu au moins NB_RELANCES_MAX relances,
    // annule tout rendez-vous FUTUR encore planifie pour ce patient
    // (le medecin redevient disponible sur ce creneau).
    // Ne touche jamais au rendez-vous deja passe (celui qui a genere
    // la consultation impayee) : seuls les rendez-vous a venir sont
    // annules, conformement a la regle "un patient endette ne garde
    // pas ses rendez-vous futurs tant qu'il n'a pas regularise".
    // ---------------------------------------------------------------
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

            // Retrouve le patient concerne par cette facture.
            string queryPatient = @"
                SELECT r.ID FROM PAIEMENT pa
                INNER JOIN CONSULTATION c ON pa.NUMEROCONSULTATION = c.NUMEROCONSULTATION
                INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
                WHERE pa.NUMEROPAIEMENT = @NumeroPaiement;";

            string patientId;
            using (var cmdPatient = new NpgsqlCommand(queryPatient, conn))
            {
                cmdPatient.Parameters.AddWithValue("NumeroPaiement", facture.NumeroPaiement);
                patientId = (string)cmdPatient.ExecuteScalar()!;
            }

            // Rendez-vous futurs encore planifies pour ce patient.
            string queryFuturs = @"
                SELECT NUMERORDV FROM RENDEZ_VOUS
                WHERE ID = @PatientId AND STATUT = 'PLANIFIE' AND DATEHEURERDV > now();";

            var rdvAAnnuler = new List<string>();
            using (var cmdFuturs = new NpgsqlCommand(queryFuturs, conn))
            {
                cmdFuturs.Parameters.AddWithValue("PatientId", patientId);
                using var reader = cmdFuturs.ExecuteReader();
                while (reader.Read()) rdvAAnnuler.Add(reader.GetString(0));
            }

            foreach (var numeroRdv in rdvAAnnuler)
            {
                string queryAnnuler = @"
                    UPDATE RENDEZ_VOUS
                    SET STATUT = 'ANNULE',
                        MOTIFANNULATION = @Motif
                    WHERE NUMERORDV = @NumeroRdv;";

                using var cmdAnnuler = new NpgsqlCommand(queryAnnuler, conn);
                cmdAnnuler.Parameters.AddWithValue("Motif", $"Annulation automatique : facture {facture.NumeroPaiement} impayée depuis {facture.JoursEcoules} jours après {facture.NombreRelances} relance(s).");
                cmdAnnuler.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                cmdAnnuler.ExecuteNonQuery();

                actions.Add($"Rendez-vous {numeroRdv} ({facture.PatientNom}) annulé automatiquement pour impayé.");
            }

            if (rdvAAnnuler.Count == 0)
            {
                actions.Add($"Facture {facture.NumeroPaiement} ({facture.PatientNom}) toujours impayée après délai — aucun rendez-vous futur à annuler pour ce patient.");
            }
        }

        if (actions.Count == 0)
            actions.Add("Aucune action nécessaire : pas de facture en retard au-delà des seuils configurés.");

        return actions;
    }
}