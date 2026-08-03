using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PaiementService
{
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
}
