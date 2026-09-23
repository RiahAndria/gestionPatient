using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public partial class ConsultationService
    {
        public Consultation? ObtenirParNumeroRendezVous(string numeroRendezVous)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = @"
                SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
                       o.NUMEROPRESCRITPTION, o.TRAITEMENT, o.DUREE
                FROM CONSULTATION c
                LEFT JOIN ORDONANCE o ON c.NUMEROCONSULTATION = o.NUMEROCONSULTATION
                WHERE c.NUMERORDV = @numRdv;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@numRdv", numeroRendezVous);

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

        public Consultation? ObtenirParNumero(string numeroConsultation)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = @"
                SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
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