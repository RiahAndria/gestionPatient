using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public class ConsultationService
    {
        private readonly string _connectionString;

        public ConsultationService()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        /// <summary>
        /// Enregistre une consultation, son ordonnance facultative et synchronise les informations du dossier médical.
        /// </summary>
        public bool EnregistrerConsultation(Consultation consultation, Ordonnance? ordonnance)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
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

                        if (!string.IsNullOrWhiteSpace(consultation.NumeroDossier))
                        {
                            MettreAJourDossierMedical(consultation, conn, transaction);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"[Erreur ADO.NET] : {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public Dossier? ObtenirDossierParNumero(string numeroDossier)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string query = @"
                SELECT NUMERODOSSIER, POIDS, TAILLE, GROUPESANGUIN, ALLERGIES, ANTECEDENTS
                FROM DOSSIER_MEDICAL
                WHERE NUMERODOSSIER = @numeroDossier;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@numeroDossier", numeroDossier);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Dossier
            {
                NumeroDossier = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                Poids = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                Taille = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                GroupeSanguin = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Allergies = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Antecedents = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Traitement = string.Empty
            };
        }

        public Dossier ConstruireDossierDepuisConsultation(Consultation consultation)
        {
            return new Dossier
            {
                NumeroDossier = consultation.NumeroDossier,
                Poids = consultation.Poids ?? 0m,
                Taille = consultation.Taille ?? 0m,
                GroupeSanguin = consultation.GroupeSanguin,
                Allergies = consultation.Allergies,
                Traitement = consultation.Traitement,
                Antecedents = consultation.Antecedents
            };
        }

        private void MettreAJourDossierMedical(Consultation consultation, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string checkSql = "SELECT COUNT(*) FROM DOSSIER_MEDICAL WHERE NUMERODOSSIER = @numeroDossier;";
            using var checkCmd = new NpgsqlCommand(checkSql, conn, transaction);
            checkCmd.Parameters.AddWithValue("@numeroDossier", consultation.NumeroDossier);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists > 0)
            {
                string updateSql = @"
                    UPDATE DOSSIER_MEDICAL
                    SET POIDS = @poids,
                        TAILLE = @taille,
                        GROUPESANGUIN = @groupeSanguin,
                        ALLERGIES = @allergies,
                        ANTECEDENTS = @antecedents
                    WHERE NUMERODOSSIER = @numeroDossier;";

                using var cmd = new NpgsqlCommand(updateSql, conn, transaction);
                cmd.Parameters.AddWithValue("@numeroDossier", consultation.NumeroDossier);
                cmd.Parameters.AddWithValue("@poids", consultation.Poids.HasValue ? consultation.Poids.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@taille", consultation.Taille.HasValue ? consultation.Taille.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@groupeSanguin", string.IsNullOrWhiteSpace(consultation.GroupeSanguin) ? DBNull.Value : consultation.GroupeSanguin);
                cmd.Parameters.AddWithValue("@allergies", string.IsNullOrWhiteSpace(consultation.Allergies) ? DBNull.Value : consultation.Allergies);
                cmd.Parameters.AddWithValue("@antecedents", string.IsNullOrWhiteSpace(consultation.Antecedents) ? DBNull.Value : consultation.Antecedents);
                cmd.ExecuteNonQuery();
            }
            else
            {
                string insertSql = @"
                    INSERT INTO DOSSIER_MEDICAL (NUMERODOSSIER, POIDS, TAILLE, GROUPESANGUIN, ALLERGIES, ANTECEDENTS)
                    VALUES (@numeroDossier, @poids, @taille, @groupeSanguin, @allergies, @antecedents);";

                using var cmd = new NpgsqlCommand(insertSql, conn, transaction);
                cmd.Parameters.AddWithValue("@numeroDossier", consultation.NumeroDossier);
                cmd.Parameters.AddWithValue("@poids", consultation.Poids.HasValue ? consultation.Poids.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@taille", consultation.Taille.HasValue ? consultation.Taille.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@groupeSanguin", string.IsNullOrWhiteSpace(consultation.GroupeSanguin) ? DBNull.Value : consultation.GroupeSanguin);
                cmd.Parameters.AddWithValue("@allergies", string.IsNullOrWhiteSpace(consultation.Allergies) ? DBNull.Value : consultation.Allergies);
                cmd.Parameters.AddWithValue("@antecedents", string.IsNullOrWhiteSpace(consultation.Antecedents) ? DBNull.Value : consultation.Antecedents);
                cmd.ExecuteNonQuery();
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