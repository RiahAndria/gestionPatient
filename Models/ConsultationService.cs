using System;
using System.Collections.Generic;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public class ConsultationService
    {
        // Chaîne de connexion PostgreSQL (à adapter selon vos identifiants)
        private readonly string _connectionString = "Host=localhost;Database=patients_db;Username=postgres;Password=votre_mot_de_passe";

        /// <summary>
        /// Enregistre une consultation et son ordonnance facultative dans une transaction ADO.NET.
        /// </summary>
        public bool EnregistrerConsultation(Consultation consultation, Ordonnance? ordonnance)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                // Début de la transaction ADO.NET
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertion de la CONSULTATION
                        string sqlConsultation = @"
                            INSERT INTO CONSULTATION (NUMEROCONSULTATION, DIAGNOSTIQUE, NOTESMEDICALES)
                            VALUES (@numCons, @diag, @notes);";

                        using (var cmd = new NpgsqlCommand(sqlConsultation, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
                            cmd.Parameters.AddWithValue("@diag", consultation.Diagnostique);
                            cmd.Parameters.AddWithValue("@notes", consultation.NotesMedicales);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Insertion de l'ORDONANCE (si elle existe)
                        if (ordonnance != null)
                        {
                            string sqlOrdonnance = @"
                                INSERT INTO ORDONANCE (NUMEROPRESCRIPTION, NUMEROCONSULTATION, TRAITEMENT, DUREE, DIAGNOSTIQUE)
                                VALUES (@numPresc, @numCons, @traitement, @duree, @diag);";

                            using (var cmdOrd = new NpgsqlCommand(sqlOrdonnance, conn, transaction))
                            {
                                cmdOrd.Parameters.AddWithValue("@numPresc", ordonnance.NumeroPrescription);
                                cmdOrd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
                                cmdOrd.Parameters.AddWithValue("@traitement", ordonnance.Traitement);
                                cmdOrd.Parameters.AddWithValue("@duree", ordonnance.Duree);
                                cmdOrd.Parameters.AddWithValue("@diag", ordonnance.Diagnostique);
                                cmdOrd.ExecuteNonQuery();
                            }
                        }

                        // Validation définitive
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Annulation en cas d'erreur
                        transaction.Rollback();
                        Console.WriteLine($"[Erreur ADO.NET] : {ex.Message}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Récupère une consultation et son ordonnance via un NpgsqlDataReader.
        /// </summary>
        public Consultation? ObtenirParNumero(string numeroConsultation)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
                           o.NUMEROPRESCRIPTION, o.TRAITEMENT, o.DUREE
                    FROM CONSULTATION c
                    LEFT JOIN ORDONANCE o ON c.NUMEROCONSULTATION = o.NUMEROCONSULTATION
                    WHERE c.NUMEROCONSULTATION = @id;";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", numeroConsultation);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var consultation = new Consultation
                            {
                                NumeroConsultation = reader.GetString(0),
                                Diagnostique = reader.GetString(1),
                                NotesMedicales = reader.GetString(2)
                            };

                            // Vérification si une ordonnance est associée (colonne 3 non NULL)
                            if (!reader.IsDBNull(3))
                            {
                                consultation.OrdonnanceAssociee = new Ordonnance
                                {
                                    NumeroPrescription = reader.GetString(3),
                                    NumeroConsultation = consultation.NumeroConsultation,
                                    Traitement = reader.GetString(4),
                                    Duree = reader.GetString(5),
                                    Diagnostique = consultation.Diagnostique
                                };
                            }

                            return consultation;
                        }
                    }
                }
            }
            return null;
        }
    }
}