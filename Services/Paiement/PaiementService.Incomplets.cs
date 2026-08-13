using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PaiementService
{
    // Liste unifiee pour la section "Paiements non complets" de la
    // page Paiements (voir Models/PaiementIncomplet.cs) : combine
    //   1) les factures NORMALES (solde post-consultation) pas encore
    //      payees (PAIEMENT.STATUT = false) ;
    //   2) les rendez-vous encore planifies pour lesquels un acompte a
    //      ete verse mais ne couvre pas (encore) tout le tarif du
    //      medecin.
    public List<PaiementIncomplet> ObtenirPaiementsIncomplets()
    {
        var resultat = new List<PaiementIncomplet>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        // 1) Soldes normaux impayes.
        string queryNormal = @"
            SELECT pa.NUMEROPAIEMENT, r.NUMERORDV, pp.NOM, pp.PRENOM, pa.MONTANT, r.DATEHEURERDV,
                   (SELECT COUNT(*) FROM NOTIFICATION n WHERE n.NUMERORDV = r.NUMERORDV AND n.TYPE_NOTIF = 'PAIEMENT') AS NBALERTES
            FROM PAIEMENT pa
            INNER JOIN RENDEZ_VOUS r ON pa.NUMERORDV = r.NUMERORDV
            INNER JOIN PATIENT pat ON r.ID = pat.ID
            INNER JOIN PERSONNE pp ON pat.ID = pp.ID
            WHERE pa.STATUT = false;";

        using (var cmd = new NpgsqlCommand(queryNormal, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                resultat.Add(new PaiementIncomplet
                {
                    NumeroPaiement = reader.GetString(0),
                    NumeroRdv = reader.GetString(1),
                    PatientNom = $"{reader.GetString(2)} {reader.GetString(3)}",
                    MontantRestant = reader.GetDecimal(4),
                    DateLimite = reader.GetDateTime(5),
                    NombreAlertes = (int)reader.GetInt64(6)
                });
            }
        }

        // 2) Acomptes partiels sur des RDV pas encore realises.
        string queryAcompte = @"
            SELECT r.NUMERORDV, pp.NOM, pp.PRENOM, me.TAUX_HORAIRE,
                   COALESCE((SELECT SUM(MONTANT) FROM PAIEMENT
                             WHERE NUMERORDV = r.NUMERORDV AND TYPEPAIEMENT = 'ACOMPTE' AND STATUT = true), 0) AS DEJAVERSE,
                   r.DATEHEURERDV,
                   (SELECT COUNT(*) FROM NOTIFICATION n WHERE n.NUMERORDV = r.NUMERORDV AND n.TYPE_NOTIF = 'PAIEMENT') AS NBALERTES
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pat ON r.ID = pat.ID
            INNER JOIN PERSONNE pp ON pat.ID = pp.ID
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            WHERE r.STATUT = 'PLANIFIE'
              AND EXISTS (SELECT 1 FROM PAIEMENT WHERE NUMERORDV = r.NUMERORDV AND TYPEPAIEMENT = 'ACOMPTE' AND STATUT = true)
              AND NOT EXISTS (SELECT 1 FROM PAIEMENT WHERE NUMERORDV = r.NUMERORDV AND TYPEPAIEMENT = 'NORMAL');";

        using (var cmd = new NpgsqlCommand(queryAcompte, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                decimal tarif = reader.GetDecimal(3);
                decimal dejaVerse = reader.GetDecimal(4);
                decimal reste = tarif - dejaVerse;
                if (reste <= 0) continue; // deja entierement couvert par l'acompte

                resultat.Add(new PaiementIncomplet
                {
                    NumeroPaiement = null,
                    NumeroRdv = reader.GetString(0),
                    PatientNom = $"{reader.GetString(1)} {reader.GetString(2)}",
                    MontantRestant = reste,
                    DateLimite = reader.GetDateTime(5),
                    NombreAlertes = (int)reader.GetInt64(6)
                });
            }
        }

        return resultat.OrderBy(p => p.DateLimite).ToList();
    }
}
