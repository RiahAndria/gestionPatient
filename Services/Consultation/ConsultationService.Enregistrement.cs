using System;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public partial class ConsultationService
    {
        public bool EnregistrerConsultation(Consultation consultation, Ordonnance? ordonnance)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                InsererConsultation(consultation, conn, transaction);

                if (ordonnance != null)
                {
                    InsererOrdonnance(consultation, ordonnance, conn, transaction);
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

        private static void InsererConsultation(Consultation consultation, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            const string sqlConsultation = @"
                INSERT INTO CONSULTATION (NUMEROCONSULTATION, DIAGNOSTIQUE, NOTESMEDICALES)
                VALUES (@numCons, @diag, @notes);";

            using var cmd = new NpgsqlCommand(sqlConsultation, conn, transaction);
            cmd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
            cmd.Parameters.AddWithValue("@diag", consultation.Diagnostique);
            cmd.Parameters.AddWithValue("@notes", consultation.NotesMedicales);
            cmd.ExecuteNonQuery();
        }

        private static void InsererOrdonnance(Consultation consultation, Ordonnance ordonnance, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            const string sqlOrdonnance = @"
                INSERT INTO ORDONANCE (NUMEROPRESCRIPTION, NUMEROCONSULTATION, TRAITEMENT, DUREE, DIAGNOSTIQUE)
                VALUES (@numPresc, @numCons, @traitement, @duree, @diag);";

            using var cmdOrd = new NpgsqlCommand(sqlOrdonnance, conn, transaction);
            cmdOrd.Parameters.AddWithValue("@numPresc", ordonnance.NumeroPrescritption);
            cmdOrd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
            cmdOrd.Parameters.AddWithValue("@traitement", ordonnance.Traitement);
            cmdOrd.Parameters.AddWithValue("@duree", ordonnance.Duree);
            cmdOrd.Parameters.AddWithValue("@diag", ordonnance.Diagnostique);
            cmdOrd.ExecuteNonQuery();
        }
    }
}
