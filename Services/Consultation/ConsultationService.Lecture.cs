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
                SELECT c.NUMEROCONSULTATION, c.NUMERORDV, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
                       o.NUMEROPRESCRITPTION, o.TRAITEMENT, o.DUREE
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
                NumeroRdv = reader.GetString(1),
                Diagnostique = reader.GetString(2),
                NotesMedicales = reader.GetString(3)
            };

            if (!reader.IsDBNull(4))
            {
                consultation.OrdonnanceAssociee = new Ordonnance
                {
                    NumeroPrescritption = reader.GetString(4),
                    NumeroConsultation = consultation.NumeroConsultation,
                    Traitement = reader.GetString(5),
                    Duree = reader.GetString(6),
                    Diagnostique = consultation.Diagnostique
                };
            }

            return consultation;
        }
    }
}
