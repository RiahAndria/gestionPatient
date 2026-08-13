using Npgsql;

namespace Patients.Services;

public partial class RappelService
{
    // Cree une notification ponctuelle liee a un rendez-vous, sur le
    // meme principe que GenererRappels24h / EnvoyerRelance mais
    // declenchee immediatement (pas par un job planifie). Utilisee par
    // l'etape 6 de l'assistant de rendez-vous juste apres validation
    // du paiement, pour alerter le secretariat / le patient.
    public void CreerNotification(string numeroRdv, string message, string typeNotif = "RESERVATION")
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string idNotif = "NOTIF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        string insertQuery = @"
            INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU, TYPE_NOTIF)
            VALUES (@numNotif, @numRdv, @texte, now(), false, @type);";

        using var cmd = new NpgsqlCommand(insertQuery, conn);
        cmd.Parameters.AddWithValue("@numNotif", idNotif);
        cmd.Parameters.AddWithValue("@numRdv", numeroRdv);
        cmd.Parameters.AddWithValue("@texte", message);
        cmd.Parameters.AddWithValue("@type", typeNotif);
        cmd.ExecuteNonQuery();
    }

    // Bouton "🔔" de la grille des rendez-vous (page Rendez-vous) :
    // cree une alerte manuelle pour ce RDV et retourne le nouveau
    // nombre total d'alertes envoyees pour lui.
    public int CreerAlerteRendezVous(string numeroRdv)
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

        int joursRestants = (dateHeure.Date - DateTime.Today).Days;
        string message = $"Rappel RDV : M./Mme {nomComplet} le {dateHeure:dd/MM/yyyy à HH:mm} (J-{joursRestants}).";

        string idNotif = "NOTIF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        string insertQuery = @"
            INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU, TYPE_NOTIF)
            VALUES (@numNotif, @numRdv, @texte, now(), false, 'RESERVATION');";

        using (var cmdInsert = new NpgsqlCommand(insertQuery, conn))
        {
            cmdInsert.Parameters.AddWithValue("@numNotif", idNotif);
            cmdInsert.Parameters.AddWithValue("@numRdv", numeroRdv);
            cmdInsert.Parameters.AddWithValue("@texte", message);
            cmdInsert.ExecuteNonQuery();
        }

        return CompterAlertesRendezVous(conn, numeroRdv);
    }

    public int CompterAlertesRendezVous(string numeroRdv)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        return CompterAlertesRendezVous(conn, numeroRdv);
    }

    private int CompterAlertesRendezVous(NpgsqlConnection conn, string numeroRdv)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM NOTIFICATION WHERE NUMERORDV = @NumeroRdv AND TYPE_NOTIF = 'RESERVATION';", conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        return (int)(long)cmd.ExecuteScalar()!;
    }
}
