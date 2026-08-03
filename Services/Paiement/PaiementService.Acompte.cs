using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PaiementService
{
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

    // Encaisse un acompte pour un rendez-vous pas encore realise, avec
    // validation par rapport au tarif du medecin (cumule avec les
    // acomptes deja verses sur CE rendez-vous) :
    //   - total > tarif  -> refuse (erreur claire, rien n'est enregistre)
    //   - total == tarif -> enregistre, PaiementComplet = true
    //   - total < tarif  -> enregistre, PaiementComplet = false, le
    //                       reste sera facture au moment de la
    //                       consultation (avec relances/alertes si non
    //                       regle a temps, comme n'importe quelle facture)
    public ResultatAcompte EncaisserAcompte(string numeroRdv, decimal montant, string modePaiement)
    {
        if (montant <= 0)
        {
            return new ResultatAcompte { Succes = false, MessageErreur = "Le montant doit être supérieur à zéro." };
        }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        decimal tarif = CalculerMontantSuggereParRdvInterne(conn, numeroRdv);
        decimal dejaVerse = ObtenirTotalAcomptes(conn, numeroRdv);
        decimal nouveauTotal = dejaVerse + montant;

        if (tarif <= 0)
        {
            return new ResultatAcompte { Succes = false, MessageErreur = "Impossible de déterminer le tarif du médecin pour ce rendez-vous." };
        }

        if (nouveauTotal > tarif)
        {
            decimal maxAcceptable = tarif - dejaVerse;
            return new ResultatAcompte
            {
                Succes = false,
                MessageErreur = $"Le montant dépasse le tarif du médecin ({tarif:N0} Ar). " +
                                 (dejaVerse > 0
                                     ? $"{dejaVerse:N0} Ar déjà versés, il reste au maximum {maxAcceptable:N0} Ar à encaisser."
                                     : $"Le maximum encaissable est {maxAcceptable:N0} Ar.")
            };
        }

        string numeroPaiement = $"PAI-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        string query = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMERORDV, NUMEROCONSULTATION, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroRdv, NULL, 'ACOMPTE', now(), @Montant, @Mode, true);";

        using (var cmd = new NpgsqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
            cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            cmd.Parameters.AddWithValue("Montant", montant);
            cmd.Parameters.AddWithValue("Mode", modePaiement);
            cmd.ExecuteNonQuery();
        }

        bool complet = nouveauTotal >= tarif;
        return new ResultatAcompte
        {
            Succes = true,
            PaiementComplet = complet,
            MontantRestant = Math.Max(0, tarif - nouveauTotal)
        };
    }

    // Total deja verse en acompte pour un rendez-vous donne (utilise
    // par cette classe et par PaiementService.Facture.cs).
    private decimal ObtenirTotalAcomptes(NpgsqlConnection conn, string numeroRdv)
    {
        string query = @"
            SELECT COALESCE(SUM(MONTANT), 0) FROM PAIEMENT
            WHERE NUMERORDV = @NumeroRdv AND TYPEPAIEMENT = 'ACOMPTE' AND STATUT = true;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        return (decimal)cmd.ExecuteScalar()!;
    }
}
