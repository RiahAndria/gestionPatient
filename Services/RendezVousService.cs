using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class RendezVousService
{
    private readonly string _connectionString;

    public RendezVousService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // Liste des rendez-vous, avec les noms de patient/medecin deja
    // joints. Chaque parametre de filtre est optionnel : une chaine
    // vide ou une date nulle signifie "pas de filtre sur ce critere".
    public List<RendezVousAffichage> Rechercher(string? terme, DateTime? date, string? statut)
    {
        var resultat = new List<RendezVousAffichage>();

        string query = @"
            SELECT r.NUMERORDV, 
                   pp.NOM, pp.PRENOM,
                   mp.NOM, mp.PRENOM,
                   r.DATEHEURERDV, r.MOTIFRDV, r.STATUT
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE pp ON pa.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            INNER JOIN PERSONNE mp ON me.ID_MEDECIN = mp.ID
            WHERE (@Terme = '' OR pp.NOM ILIKE '%' || @Terme || '%' OR pp.PRENOM ILIKE '%' || @Terme || '%'
                                OR mp.NOM ILIKE '%' || @Terme || '%' OR mp.PRENOM ILIKE '%' || @Terme || '%')
              AND (@DateFiltre::date IS NULL OR r.DATEHEURERDV::date = @DateFiltre::date)
              AND (@Statut = '' OR r.STATUT = @Statut)
            ORDER BY r.DATEHEURERDV;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Terme", terme ?? "");
        cmd.Parameters.AddWithValue("DateFiltre", (object?)date ?? DBNull.Value);
        cmd.Parameters.AddWithValue("Statut", statut ?? "");

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

    public List<RendezVousAffichage> ObtenirTous() => Rechercher(terme: "", date: null, statut: "");

    // Toutes les informations d'un rendez-vous pour la fenetre de detail
    // (ouverte par double-clic) : patient, medecin et leurs coordonnees.
    public RendezVousDetail? ObtenirDetail(string numeroRdv)
    {
        string query = @"
            SELECT r.NUMERORDV, r.DATEHEURERDV, r.MOTIFRDV, r.STATUT, r.MOTIFANNULATION,
                   pp.ID, pp.NOM, pp.PRENOM, pp.TELEPHONE, pp.MAIL, pa.NUMERODOSSIER,
                   mp.NOM, mp.PRENOM, f.NOM_FONCTION, me.TAUX_HORAIRE
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE pp ON pa.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            INNER JOIN PERSONNE mp ON me.ID_MEDECIN = mp.ID
            INNER JOIN FONCTION f ON me.CODE_FONCTION = f.CODE_FONCTION
            WHERE r.NUMERORDV = @NumeroRdv;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new RendezVousDetail
        {
            NumeroRdv = reader.GetString(0),
            DateHeure = reader.GetDateTime(1),
            Motif = reader.GetString(2),
            Statut = reader.GetString(3),
            MotifAnnulation = reader.IsDBNull(4) ? null : reader.GetString(4),
            PatientId = reader.GetString(5),
            PatientNom = $"{reader.GetString(6)} {reader.GetString(7)}",
            PatientTelephone = reader.GetString(8),
            PatientEmail = reader.GetString(9),
            PatientMatricule = reader.GetString(10),
            MedecinNom = $"Dr. {reader.GetString(11)} {reader.GetString(12)}",
            MedecinFonction = reader.GetString(13),
            MedecinTauxHoraire = reader.GetDecimal(14)
        };
    }

    // Regle metier : un medecin ne peut pas avoir deux rendez-vous
    // planifies au meme instant. exclureNumeroRdv sert lors d'une
    // reprogrammation, pour ne pas comparer le rendez-vous a lui-meme.
    private bool CreneauDejaPris(NpgsqlConnection conn, NpgsqlTransaction tx, string medecinId, DateTime dateHeure, string? exclureNumeroRdv)
    {
        string query = @"
            SELECT COUNT(*) FROM RENDEZ_VOUS
            WHERE ID_HER_2 = @MedecinId
              AND DATEHEURERDV = @DateHeure
              AND STATUT = 'PLANIFIE'
              AND (@ExclureNumero = '' OR NUMERORDV <> @ExclureNumero);";

        using var cmd = new NpgsqlCommand(query, conn, tx);
        cmd.Parameters.AddWithValue("MedecinId", medecinId);
        cmd.Parameters.AddWithValue("DateHeure", dateHeure);
        cmd.Parameters.AddWithValue("ExclureNumero", exclureNumeroRdv ?? "");

        var count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    // Cree un rendez-vous. Leve une InvalidOperationException si le
    // creneau est deja pris pour ce medecin.
    public void AjouterRendezVous(RendezVous rdv)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            if (CreneauDejaPris(conn, transaction, rdv.MedecinID, rdv.DateHeure, exclureNumeroRdv: null))
            {
                throw new InvalidOperationException("Ce médecin a déjà un rendez-vous planifié à ce créneau.");
            }

            string query = @"
                INSERT INTO RENDEZ_VOUS (NUMERORDV, ID, ID_HER_2, DATEHEURERDV, MOTIFRDV, STATUT)
                VALUES (@NumeroRdv, @PatientId, @MedecinId, @DateHeure, @Motif, 'PLANIFIE');";

            using var cmd = new NpgsqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("NumeroRdv", rdv.NumRendezVous);
            cmd.Parameters.AddWithValue("PatientId", rdv.PatientID);
            cmd.Parameters.AddWithValue("MedecinId", rdv.MedecinID);
            cmd.Parameters.AddWithValue("DateHeure", rdv.DateHeure);
            cmd.Parameters.AddWithValue("Motif", rdv.Motif);
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // La ligne n'est jamais supprimee (on garde l'historique) : on
    // passe simplement le statut a ANNULE avec le motif fourni.
    public void AnnulerRendezVous(string numeroRdv, string motif)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE RENDEZ_VOUS
            SET STATUT = 'ANNULE', MOTIFANNULATION = @Motif
            WHERE NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Motif", motif);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }

    // Change la date/heure d'un rendez-vous, en revalidant qu'aucun
    // conflit de creneau n'est cree. Refuse si une consultation est deja
    // rattachee (contrainte UNIQUE CONSULTATION.NUMERORDV) : il faut
    // alors annuler puis creer un nouveau rendez-vous.
    public void ReprogrammerRendezVous(string numeroRdv, DateTime nouvelleDateHeure)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string queryVerifConsultation = "SELECT COUNT(*) FROM CONSULTATION WHERE NUMERORDV = @NumeroRdv;";
            using (var cmdVerif = new NpgsqlCommand(queryVerifConsultation, conn, transaction))
            {
                cmdVerif.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                var nbConsultations = (long)cmdVerif.ExecuteScalar()!;
                if (nbConsultations > 0)
                {
                    throw new InvalidOperationException(
                        "Ce rendez-vous a déjà une consultation associée : annule-le et crée un nouveau rendez-vous plutôt que de le reprogrammer.");
                }
            }

            string queryMedecin = "SELECT ID_HER_2 FROM RENDEZ_VOUS WHERE NUMERORDV = @NumeroRdv;";
            string medecinId;
            using (var cmdMedecin = new NpgsqlCommand(queryMedecin, conn, transaction))
            {
                cmdMedecin.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                medecinId = (string)(cmdMedecin.ExecuteScalar() ?? throw new InvalidOperationException("Rendez-vous introuvable."));
            }

            if (CreneauDejaPris(conn, transaction, medecinId, nouvelleDateHeure, exclureNumeroRdv: numeroRdv))
            {
                throw new InvalidOperationException("Ce médecin a déjà un rendez-vous planifié sur ce nouveau créneau.");
            }

            string queryUpdate = "UPDATE RENDEZ_VOUS SET DATEHEURERDV = @NouvelleDate WHERE NUMERORDV = @NumeroRdv;";
            using var cmdUpdate = new NpgsqlCommand(queryUpdate, conn, transaction);
            cmdUpdate.Parameters.AddWithValue("NouvelleDate", nouvelleDateHeure);
            cmdUpdate.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            cmdUpdate.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
