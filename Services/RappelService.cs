using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public class RappelService
    {
        private readonly string _connectionString;

        public RappelService()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

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
}
