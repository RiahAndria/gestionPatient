using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class PaiementService
{
    private readonly string _connectionString;

    // Seuils de la politique de relance/annulation, modifiables ici en
    // un seul endroit.
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

    // -----------------------------------------------------------------
    // PAIEMENT EN AVANCE (acompte)
    // -----------------------------------------------------------------

    // Rendez-vous encore planifies (pas encore de consultation, donc
    // pas encore eu lieu) : ce sont les seuls eligibles a un acompte.
    public List<RendezVousAffichage> ObtenirRendezVousEligiblesAcompte()
    {
        var resultat = new List<RendezVousAffichage>();

        string query = @"
            SELECT r.NUMERORDV, pp.NOM, pp.PRENOM, mp.NOM, mp.PRENOM, r.DATEHEURERDV, r.MOTIFRDV, r.STATUT
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE pp ON pa.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            INNER JOIN PERSONNE mp ON me.ID_MEDECIN = mp.ID
            WHERE r.STATUT = 'PLANIFIE'
            ORDER BY r.DATEHEURERDV;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new RendezVousAffichage
            {
                NumeroRdv = reader.GetString(0),
                PatientNom = $"{reader.GetString(1)} {reader.GetString(2)}",
                MedecinNom = $"Dr. {reader.GetString(3)} {reader.GetString(4)}",
                DateHeure = reader.GetDateTime(5),
                Motif = reader.GetString(6),
                Statut = reader.GetString(7)
            });
        }

        return resultat;
    }

    // Encaisse un acompte pour un rendez-vous pas encore realise.
    // Contrairement au paiement "normal", l'acompte est considere payé
    // immediatement (on l'encaisse au moment ou l'utilisateur le saisit,
    // pas de facture "en attente" pour un acompte).
    public void EncaisserAcompte(string numeroRdv, decimal montant, string modePaiement)
    {
        string numeroPaiement = $"PAI-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        string query = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMERORDV, NUMEROCONSULTATION, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroRdv, NULL, 'ACOMPTE', now(), @Montant, @Mode, true);";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.Parameters.AddWithValue("Montant", montant);
        cmd.Parameters.AddWithValue("Mode", modePaiement);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    // Total deja verse en acompte pour un rendez-vous donne (utilise
    // pour deduire le solde restant au moment de la facture finale).
    private decimal ObtenirTotalAcomptes(NpgsqlConnection conn, string numeroRdv)
    {
        string query = @"
            SELECT COALESCE(SUM(MONTANT), 0) FROM PAIEMENT
            WHERE NUMERORDV = @NumeroRdv AND TYPEPAIEMENT = 'ACOMPTE' AND STATUT = true;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        return (decimal)cmd.ExecuteScalar()!;
    }

    // -----------------------------------------------------------------
    // PAIEMENT NORMAL (solde, apres consultation)
    // -----------------------------------------------------------------

    // A appeler par le module Consultation juste apres l'enregistrement
    // d'une consultation : cree la facture en attente pour le solde
    // restant (montant total moins les acomptes deja verses pour ce
    // rendez-vous).
    public void CreerPaiementDu(string numeroConsultation, decimal montantTotal, string modePaiementPropose = "Non précisé")
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = "SELECT NUMERORDV FROM CONSULTATION WHERE NUMEROCONSULTATION = @NumeroConsultation;";
        string numeroRdv;
        using (var cmdRdv = new NpgsqlCommand(queryRdv, conn))
        {
            cmdRdv.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
            var resultat = cmdRdv.ExecuteScalar();
            if (resultat is null)
                throw new InvalidOperationException("Impossible de créer un paiement : la consultation n'existe pas encore (pas de paiement en avance).");
            numeroRdv = (string)resultat;
        }

        decimal acomptesDejaVerses = ObtenirTotalAcomptes(conn, numeroRdv);
        decimal solde = Math.Max(0, montantTotal - acomptesDejaVerses);

        string numeroPaiement = $"PAI-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        string query = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMERORDV, NUMEROCONSULTATION, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroRdv, @NumeroConsultation, 'NORMAL', now(), @Montant, @Mode, false);";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
        cmd.Parameters.AddWithValue("Montant", solde);
        cmd.Parameters.AddWithValue("Mode", modePaiementPropose);
        cmd.ExecuteNonQuery();
    }

    // Montant total suggere = taux horaire du medecin qui a tenu la
    // consultation (tarif forfaitaire par consultation, faute d'une
    // duree stockee dans le schema). C'est le montant AVANT deduction
    // des acomptes - CreerPaiementDu se charge de la deduction.
    public decimal CalculerMontantSuggere(string numeroConsultation)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            SELECT me.TAUX_HORAIRE
            FROM CONSULTATION c
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            WHERE c.NUMEROCONSULTATION = @NumeroConsultation;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
        var resultat = cmd.ExecuteScalar();
        return resultat is decimal montant ? montant : 0m;
    }

    // -----------------------------------------------------------------
    // LECTURE (factures en attente, historique, detail par RDV)
    // -----------------------------------------------------------------

    public List<PaiementAffichage> ObtenirEnAttente()
    {
        var resultat = new List<PaiementAffichage>();

        string query = @"
            SELECT pa.NUMEROPAIEMENT, pa.NUMEROCONSULTATION, r.NUMERORDV, pa.TYPEPAIEMENT,
                   pp.NOM, pp.PRENOM, pa.DATEPAIEMENT, pa.MONTANT, pa.MODEPAIEMENT,
                   (SELECT COUNT(*) FROM NOTIFICATION n WHERE n.NUMERORDV = r.NUMERORDV) AS NBRELANCES
            FROM PAIEMENT pa
            INNER JOIN RENDEZ_VOUS r ON pa.NUMERORDV = r.NUMERORDV
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
                NumeroConsultation = reader.IsDBNull(1) ? "" : reader.GetString(1),
                NumeroRdv = reader.GetString(2),
                TypePaiement = reader.GetString(3),
                PatientNom = $"{reader.GetString(4)} {reader.GetString(5)}",
                DateFacture = reader.GetDateTime(6),
                Montant = reader.GetDecimal(7),
                ModePaiement = reader.GetString(8),
                EstPaye = false,
                NombreRelances = (int)reader.GetInt64(9)
            });
        }

        return resultat;
    }

    public List<PaiementAffichage> ObtenirHistoriquePayes()
    {
        var resultat = new List<PaiementAffichage>();

        string query = @"
            SELECT pa.NUMEROPAIEMENT, pa.NUMEROCONSULTATION, r.NUMERORDV, pa.TYPEPAIEMENT,
                   pp.NOM, pp.PRENOM, pa.DATEPAIEMENT, pa.MONTANT, pa.MODEPAIEMENT
            FROM PAIEMENT pa
            INNER JOIN RENDEZ_VOUS r ON pa.NUMERORDV = r.NUMERORDV
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
                NumeroConsultation = reader.IsDBNull(1) ? "" : reader.GetString(1),
                NumeroRdv = reader.GetString(2),
                TypePaiement = reader.GetString(3),
                PatientNom = $"{reader.GetString(4)} {reader.GetString(5)}",
                DateFacture = reader.GetDateTime(6),
                Montant = reader.GetDecimal(7),
                ModePaiement = reader.GetString(8),
                EstPaye = true
            });
        }

        return resultat;
    }

    // Tous les paiements (acomptes + normal, payes ou non) lies a un
    // rendez-vous precis - utilise par la fenetre de detail du RDV.
    public List<PaiementAffichage> ObtenirParRendezVous(string numeroRdv)
    {
        var resultat = new List<PaiementAffichage>();

        string query = @"
            SELECT NUMEROPAIEMENT, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT
            FROM PAIEMENT
            WHERE NUMERORDV = @NumeroRdv
            ORDER BY DATEPAIEMENT;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new PaiementAffichage
            {
                NumeroPaiement = reader.GetString(0),
                TypePaiement = reader.GetString(1),
                DateFacture = reader.GetDateTime(2),
                Montant = reader.GetDecimal(3),
                ModePaiement = reader.GetString(4),
                EstPaye = reader.GetBoolean(5)
            });
        }

        return resultat;
    }

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
    // rendez-vous futur a deja un acompte verse (dans ce cas le patient
    // s'est deja engage financierement dessus : on signale plutot une
    // verification manuelle au lieu d'annuler automatiquement).
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
