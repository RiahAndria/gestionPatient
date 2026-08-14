using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class RendezVousService
{
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
                   r.DATEHEURERDV, r.MOTIFRDV, me.STATUT,
                   (SELECT COUNT(*) FROM NOTIFICATION n WHERE n.NUMERORDV = r.NUMERORDV AND n.TYPE_NOTIF = 'RESERVATION') AS NBALERTES
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE pp ON pa.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            INNER JOIN PERSONNE mp ON me.ID_MEDECIN = mp.ID
            WHERE (@Terme = '' OR pp.NOM ILIKE '%' || @Terme || '%' OR pp.PRENOM ILIKE '%' || @Terme || '%'
                                OR mp.NOM ILIKE '%' || @Terme || '%' OR mp.PRENOM ILIKE '%' || @Terme || '%'
                                OR r.NUMERORDV ILIKE '%' || @Terme || '%')
              AND (@DateFiltre::date IS NULL OR r.DATEHEURERDV::date = @DateFiltre::date)
              AND (@Statut = '' OR me.STATUT = @Statut)
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
                Statut = reader.GetString(7),
                NombreAlertes = (int)reader.GetInt64(8)
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
            SELECT r.NUMERORDV, r.DATEHEURERDV, r.MOTIFRDV, me.STATUT, r.MOTIFANNULATION,
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
}
