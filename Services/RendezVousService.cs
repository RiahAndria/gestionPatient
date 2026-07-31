using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;
// Service pour gérer les rendez-vous
public class RendezVousService
{
    private readonly string _connectionString;
// Constructeur de la classe qui initialise la chaîne de connexion à la base de données
    public RendezVousService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
// Méthode pour rechercher les rendez-vous en fonction de différents critères
    public List<RendezVousAffichage> Rechercher(string? terme, DateTime? date, string? statut)
    {
        var resultat = new List<RendezVousAffichage>();
// Requête SQL pour récupérer les rendez-vous avec les informations du patient et du médecin
        string query = @"
            SELECT r.NUMERORDV, 
                   pp.NOM, pp.PRENOM,
                   mp.NOM, mp.PRENOM,
                   r.DATEHEURERDV, r.MOTIFRDV, r.STATUT
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE pp ON pa.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_HER_2
            INNER JOIN PERSONNE mp ON me.ID_HER_2 = mp.ID
            WHERE (@Terme = '' OR pp.NOM ILIKE '%' || @Terme || '%' OR pp.PRENOM ILIKE '%' || @Terme || '%'
                                OR mp.NOM ILIKE '%' || @Terme || '%' OR mp.PRENOM ILIKE '%' || @Terme || '%')
              AND (@DateFiltre::date IS NULL OR r.DATEHEURERDV::date = @DateFiltre::date)
              AND (@Statut = '' OR r.STATUT = @Statut)
            ORDER BY r.DATEHEURERDV;";
// On exécute la requête et on lit les résultats pour les ajouter à la liste des rendez-vous
        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Terme", terme ?? "");
        cmd.Parameters.AddWithValue("DateFiltre", (object?)date ?? DBNull.Value);
        cmd.Parameters.AddWithValue("Statut", statut ?? "");
// On ouvre la connexion et on exécute la commande
        conn.Open();
        using var reader = cmd.ExecuteReader();
// On lit chaque ligne du résultat et on crée un objet RendezVousAffichage pour l'ajouter à la liste
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
// Méthode pour obtenir tous les rendez-vous sans filtrage
    public List<RendezVousAffichage> ObtenirTous() => Rechercher(terme: "", date: null, statut: "");

    // Verifie si un medecin a deja un RDV planifie sur le meme creneau
    private bool CreneauDejaPris(NpgsqlConnection conn, NpgsqlTransaction tx, string medecinId, DateTime dateHeure, string? exclureNumeroRdv)
    {
        // On verifie si le medecin a deja un RDV planifie sur le meme creneau. On exclut le RDV en cours si on reprogramme.
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

    // Creation d'un rendez-vous. Leve une InvalidOperationException si
    // le creneau est deja pris pour ce medecin (a attraper cote UI pour
    // afficher un message clair, plutot qu'une erreur SQL brute).
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

    // Annulation : on ne supprime JAMAIS la ligne (garde l'historique),
    // on passe juste le statut a ANNULE avec le motif.
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

    public void ReprogrammerRendezVous(string numeroRdv, DateTime nouvelleDateHeure)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string queryVerifConsultation = @"
                SELECT COUNT(*) FROM CONSULTATION WHERE NUMERORDV = @NumeroRdv;";
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