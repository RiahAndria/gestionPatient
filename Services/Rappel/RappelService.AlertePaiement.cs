using Npgsql;

namespace Patients.Services;

public partial class RappelService
{
    // Bouton "🔔" de la section "Paiements non complets" (page
    // Paiements) : cree une alerte de paiement pour ce rendez-vous et
    // retourne le nouveau nombre total d'alertes envoyees pour lui.
    public int CreerAlertePaiement(string numeroRdv)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = @"
            SELECT r.DATEHEURERDV, p.NOM, p.PRENOM
            FROM RENDEZ_VOUS r
            INNER JOIN PATIENT pa ON r.ID = pa.ID
            INNER JOIN PERSONNE p ON pa.ID = p.ID
            WHERE r.NUMERORDV = @NumeroRdv;";

        DateTime dateHeure;
        string nomComplet;
        using (var cmd = new NpgsqlCommand(queryRdv, conn))
        {
            cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) throw new InvalidOperationException("Rendez-vous introuvable.");
            dateHeure = reader.GetDateTime(0);
            nomComplet = $"{reader.GetString(1)} {reader.GetString(2)}";
        }

        string message = $"M./Mme {nomComplet} : merci de compléter le règlement de votre paiement " +
                          $"avant ou pendant votre rendez-vous du {dateHeure:dd/MM/yyyy à HH:mm}.";

        string idNotif = "NOTIF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        string insertQuery = @"
            INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU, TYPE_NOTIF)
            VALUES (@numNotif, @numRdv, @texte, now(), false, 'PAIEMENT');";

        using (var cmdInsert = new NpgsqlCommand(insertQuery, conn))
        {
            cmdInsert.Parameters.AddWithValue("@numNotif", idNotif);
            cmdInsert.Parameters.AddWithValue("@numRdv", numeroRdv);
            cmdInsert.Parameters.AddWithValue("@texte", message);
            cmdInsert.ExecuteNonQuery();
        }

        using var cmdCompte = new NpgsqlCommand(
            "SELECT COUNT(*) FROM NOTIFICATION WHERE NUMERORDV = @NumeroRdv AND TYPE_NOTIF = 'PAIEMENT';", conn);
        cmdCompte.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        return (int)(long)cmdCompte.ExecuteScalar()!;
    }
}
