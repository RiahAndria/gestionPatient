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

    public List<Patients.Models.Consultation> ObtenirConsultationsParPatient(string patientId)
    {
        var consultations = new List<Patients.Models.Consultation>();

        // Le schéma actuel de la table CONSULTATION ne contient ni colonne NUMERORDV,
        // ni lien direct vers le patient ; il n'existe donc pas de jointure fiable
        // entre CONSULTATION et PATIENT dans la base. On charge l'historique disponible
        // depuis la table de consultations elle-même pour éviter la colonne inexistante.
        const string query = @"
            SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES
            FROM CONSULTATION c
            ORDER BY c.NUMEROCONSULTATION DESC;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            consultations.Add(new Patients.Models.Consultation
            {
                NumeroConsultation = reader.GetString(0),
                Diagnostique = reader.GetString(1),
                NotesMedicales = reader.GetString(2)
            });
        }

        return consultations;
    }
}
