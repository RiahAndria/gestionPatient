using Npgsql;

namespace Patients.Services;

public partial class RappelService
{
    /// <summary>
    /// Cherche les RDV des prochaines 24h et cree les notifications
    /// correspondantes (une seule par rendez-vous, jamais en double).
    /// </summary>
    public int GenererRappels24h()
    {
        int nbRappelsCrees = 0;

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = @"
            SELECT r.NUMERORDV, r.DATEHEURERDV, p.NOM, p.PRENOM 
            FROM RENDEZ_VOUS r
            JOIN PATIENT pat ON r.ID = pat.ID
            JOIN PERSONNE p ON pat.ID = p.ID
            WHERE r.STATUT = 'PLANIFIE'
              AND r.DATEHEURERDV BETWEEN CURRENT_TIMESTAMP AND (CURRENT_TIMESTAMP + INTERVAL '24 hours')
              AND r.NUMERORDV NOT IN (SELECT NUMERORDV FROM NOTIFICATION);";

        var rdvsANotifier = new List<(string NumRdv, DateTime DateHeure, string NomComplet)>();

        using (var cmd = new NpgsqlCommand(queryRdv, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                rdvsANotifier.Add((
                    reader.GetString(0),
                    reader.GetDateTime(1),
                    $"{reader.GetString(2)} {reader.GetString(3)}"
                ));
            }
        }

        foreach (var rdv in rdvsANotifier)
        {
            string idNotif = "NOTIF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string message = $"Rappel RDV : M./Mme {rdv.NomComplet} le {rdv.DateHeure:dd/MM/yyyy à HH:mm}.";

            string insertQuery = @"
                INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU)
                VALUES (@numNotif, @numRdv, @texte, now(), false);";

            using var cmdInsert = new NpgsqlCommand(insertQuery, conn);
            cmdInsert.Parameters.AddWithValue("@numNotif", idNotif);
            cmdInsert.Parameters.AddWithValue("@numRdv", rdv.NumRdv);
            cmdInsert.Parameters.AddWithValue("@texte", message);
            cmdInsert.ExecuteNonQuery();
            nbRappelsCrees++;
        }

        return nbRappelsCrees;
    }
}
