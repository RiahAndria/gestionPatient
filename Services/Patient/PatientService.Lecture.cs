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

    public List<Patients.Models.ConsultationAffichage> ObtenirConsultationsParPatient(string patientId)
    {
        var consultations = new List<Patients.Models.ConsultationAffichage>();

        const string query = @"
            SELECT c.NUMEROCONSULTATION,
                   c.DIAGNOSTIQUE,
                   c.NOTESMEDICALES,
                   c.NUMERORDV,
                   rv.DATEHEURERDV,
                   p.NOM,
                   p.PRENOM,
                   f.NOM_FONCTION
            FROM CONSULTATION c
            INNER JOIN RENDEZ_VOUS rv ON rv.NUMERORDV = c.NUMERORDV
            INNER JOIN MEDECIN m ON m.ID_MEDECIN = rv.ID_HER_2
            INNER JOIN PERSONNE p ON p.ID = m.ID_MEDECIN
            INNER JOIN FONCTION f ON f.CODE_FONCTION = m.CODE_FONCTION
            WHERE rv.ID = @PatientId
            ORDER BY rv.DATEHEURERDV DESC, c.NUMEROCONSULTATION DESC;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PatientId", patientId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            consultations.Add(new Patients.Models.ConsultationAffichage
            {
                NumeroConsultation = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                Diagnostique = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                NotesMedicales = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                NumeroRdv = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                DateConsultation = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                NomMedecin = $"{(reader.IsDBNull(5) ? string.Empty : reader.GetString(5))} {(reader.IsDBNull(6) ? string.Empty : reader.GetString(6))}".Trim(),
                FonctionMedecin = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            });
        }

        return consultations;
    }
}
