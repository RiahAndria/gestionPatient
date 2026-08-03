using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class RappelService
{
    /// <summary>
    /// Recupere toutes les notifications existantes, les plus
    /// recentes en premier, pour affichage a l'ecran.
    /// </summary>
    public List<Notification> ObtenirToutesLesNotifications()
    {
        var notifications = new List<Notification>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        string query = "SELECT NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU FROM NOTIFICATION ORDER BY DATENOTIF DESC;";

        using var cmd = new NpgsqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            notifications.Add(new Notification
            {
                NumeroNotif = reader.GetString(0),
                NumeroRdv = reader.GetString(1),
                TexteNotif = reader.GetString(2),
                DateNotif = reader.GetDateTime(3),
                Lu = reader.GetBoolean(4)
            });
        }

        return notifications;
    }

    public int CompterNonLues()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM NOTIFICATION WHERE LU = false;", conn);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public void MarquerCommeLue(string numeroNotif)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE NOTIFICATION SET LU = true WHERE NUMERONOTIF = @id;", conn);
        cmd.Parameters.AddWithValue("@id", numeroNotif);
        cmd.ExecuteNonQuery();
    }

    public void MarquerToutesCommeLues()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE NOTIFICATION SET LU = true WHERE LU = false;", conn);
        cmd.ExecuteNonQuery();
    }
}
