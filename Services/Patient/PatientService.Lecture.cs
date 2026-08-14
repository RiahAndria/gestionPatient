using System.Collections.Generic;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PatientService
{
    public List<Patients.Models.Patient> ObtenirTousLesPatients()
    {
        var liste = new List<Patients.Models.Patient>();
        const string query = @"
            SELECT p.ID, p.NOM, p.PRENOM, p.DATEDENAISSANCE, p.GENRE, p.ADRESSE, p.TELEPHONE, p.MAIL, 
                pa.NUMERODOSSIER, pa.NUMEROASSURANCE
            FROM PATIENT pa
            INNER JOIN PERSONNE p ON pa.ID = p.ID;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            liste.Add(MapToPatient(reader));
        }

        return liste;
    }

    public Dossier? ObtenirDossierMedical(string numeroDossier)
    {
        const string query = @"
            SELECT NUMERODOSSIER, POIDS, TAILLE, GROUPESANGUIN, ALLERGIES, ANTECEDENTS
            FROM DOSSIER_MEDICAL
            WHERE NUMERODOSSIER = @NumeroDossier;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@NumeroDossier", numeroDossier);

        conn.Open();
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
            Antecedents = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        };
    }

    public List<Patients.Models.Consultation> ObtenirConsultationsParPatient(string patientId)
    {
        var consultations = new List<Patients.Models.Consultation>();

        const string query = @"
            SELECT c.numeroconsultation, c.numerordv, c.diagnostique, c.notesmedicales, r.dateheurerdv
            FROM consultation c
            INNER JOIN rendez_vous r ON c.numerordv = r.numerordv
            WHERE r.id = @PatientId
            ORDER BY r.dateheurerdv DESC, c.numeroconsultation DESC;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PatientId", patientId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            consultations.Add(new Patients.Models.Consultation
            {
                NumeroConsultation = reader.GetString(0),
                NumeroRdv = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Diagnostique = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                NotesMedicales = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                DateConsultation = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
            });
        }

        return consultations;
    }
}
