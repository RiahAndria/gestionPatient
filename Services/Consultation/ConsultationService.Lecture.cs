using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public partial class ConsultationService
    {
        public Consultation? ObtenirParNumero(string numeroConsultation)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = @"
                SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
                       o.NUMEROPRESCRIPTION, o.TRAITEMENT, o.DUREE
                FROM CONSULTATION c
                LEFT JOIN ORDONANCE o ON c.NUMEROCONSULTATION = o.NUMEROCONSULTATION
                WHERE c.NUMEROCONSULTATION = @id;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", numeroConsultation);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

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
                    NumeroPrescritption = reader.GetString(3),
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
