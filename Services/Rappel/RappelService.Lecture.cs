using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class RappelService
{
    // Toutes les notifications, les plus recentes en premier. Filtre
    // optionnel par TYPE_NOTIF ('RESERVATION' ou 'PAIEMENT') pour les
    // onglets de la page Notifications ; null ou vide = tout afficher.
    public List<Notification> ObtenirNotifications(string? typeNotif = null)
    {
        var notifications = new List<Notification>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            SELECT NUMERONOTIF, NUMERORDV, TEXTENOTIF, DATENOTIF, LU, TYPE_NOTIF
            FROM NOTIFICATION
            WHERE (@Type = '' OR TYPE_NOTIF = @Type)
            ORDER BY DATENOTIF DESC;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Type", typeNotif ?? "");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            notifications.Add(new Notification
            {
                NumeroNotif = reader.GetString(0),
                NumeroRdv = reader.GetString(1),
                TexteNotif = reader.GetString(2),
                DateNotif = reader.GetDateTime(3),
                Lu = reader.GetBoolean(4),
                TypeNotif = reader.GetString(5)
            });
        }

        return notifications;
    }

    // Conserve pour compatibilite (equivaut a ObtenirNotifications(null)).
    public List<Notification> ObtenirToutesLesNotifications() => ObtenirNotifications(null);

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
