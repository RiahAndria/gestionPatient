using System;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public partial class ConsultationService
    {
        private static void MettreAJourDossierMedical(Consultation consultation, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            const string checkSql = "SELECT COUNT(*) FROM DOSSIER_MEDICAL WHERE NUMERODOSSIER = @numeroDossier;";
            using var checkCmd = new NpgsqlCommand(checkSql, conn, transaction);
            checkCmd.Parameters.AddWithValue("@numeroDossier", consultation.NumeroDossier);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists > 0)
            {
                const string updateSql = @"
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
                const string insertSql = @"
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
    }
}
