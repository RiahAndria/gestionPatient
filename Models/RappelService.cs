using System;
using System.Collections.Generic;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public class RappelService
    {
        private readonly string _connectionString = "Host=localhost;Database=patients_db;Username=postgres;Password=votre_mot_de_passe";

        /// <summary>
        /// Chercher les RDV des prochaines 24h et créer les notifications correspondantes
        /// </summary>
        public int GenererRappels24h()
        {
            int nbRappelsCrees = 0;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                // ... Sélection des RDV prévus dans les 24h qui n'ont pas encore de NOTIF
                string queryRdv = @"
                    SELECT r.NUMERORDV, r.DATEHEURERDV, p.NOM, p.PRENOM 
                    FROM RENDEZ_VOUS r
                    JOIN PATIENT pat ON r.ID = pat.ID
                    JOIN PERSONNE p ON pat.ID = p.ID
                    WHERE r.DATEHEURERDV BETWEEN CURRENT_TIMESTAMP AND (CURRENT_TIMESTAMP + INTERVAL '24 hours')
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

                // ... Insertion des nouvelles notifications
                foreach (var rdv in rdvsANotifier)
                {
                    string idNotif = "NOTIF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    string message = $"Rappel RDV : M./Mme {rdv.NomComplet} le {rdv.DateHeure:dd/MM/yyyy à HH:mm}.";

                    string insertQuery = @"
                        INSERT INTO NOTIFICATION (NUMERONOTIF, NUMERORDV, TEXTENOTIF)
                        VALUES (@numNotif, @numRdv, @texte);";

                    using (var cmdInsert = new NpgsqlCommand(insertQuery, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@numNotif", idNotif);
                        cmdInsert.Parameters.AddWithValue("@numRdv", rdv.NumRdv);
                        cmdInsert.Parameters.AddWithValue("@texte", message);

                        cmdInsert.ExecuteNonQuery();
                        nbRappelsCrees++;
                    }
                }
            }

            return nbRappelsCrees;
        }

        /// <summary>
        /// Récupèrer toutes les notifications de rappels existantes pour les afficher à l'écran
        /// </summary>
        public List<Notification> ObtenirToutesLesNotifications()
        {
            var notifications = new List<Notification>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT NUMERONOTIF, NUMERORDV, TEXTENOTIF FROM NOTIFICATION;";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new Notification
                        {
                            NumeroNotif = reader.GetString(0),
                            NumeroRdv = reader.GetString(1),
                            TexteNotif = reader.GetString(2)
                        });
                    }
                }
            }

            return notifications;
        }
    }
}