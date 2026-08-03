using System;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PatientService
{
    public void ModifierPatient(Patients.Models.Patient patient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            MettreAJourPersonne(patient, conn, transaction);
            MettreAJourPatient(patient, conn, transaction);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void MettreAJourPersonne(Patients.Models.Patient patient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string updatePersonne = @"
            UPDATE PERSONNE 
            SET NOM = @Nom, PRENOM = @Prenom, DATEDENAISSANCE = @DateNaissance, 
                GENRE = @Genre, ADRESSE = @Adresse, TELEPHONE = @Telephone, MAIL = @Mail
            WHERE ID = @Id;";

        using var cmdPers = new NpgsqlCommand(updatePersonne, conn, transaction);
        cmdPers.Parameters.AddWithValue("Nom", patient.Nom);
        cmdPers.Parameters.AddWithValue("Prenom", patient.Prenom);
        cmdPers.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
        cmdPers.Parameters.AddWithValue("Genre", patient.Genre);
        cmdPers.Parameters.AddWithValue("Adresse", patient.Adresse);
        cmdPers.Parameters.AddWithValue("Telephone", patient.Telephone);
        cmdPers.Parameters.AddWithValue("Mail", patient.Email);
        cmdPers.Parameters.AddWithValue("Id", patient.Id);
        cmdPers.ExecuteNonQuery();
    }

    private static void MettreAJourPatient(Patients.Models.Patient patient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string updatePatient = @"
            UPDATE PATIENT
            SET NUMERODOSSIER = @NumeroDossier, 
                NUMEROASSURANCE = @NumeroAssurance,
                NOM = @Nom, 
                PRENOM = @Prenom, 
                DATEDENAISSANCE = @DateNaissance,
                GENRE = @Genre, 
                ADRESSE = @Adresse, 
                TELEPHONE = @Telephone, 
                MAIL = @Mail
            WHERE ID = @Id;";

        using var cmdPat = new NpgsqlCommand(updatePatient, conn, transaction);
        cmdPat.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
        cmdPat.Parameters.AddWithValue("NumeroAssurance", patient.NumeroAssurance ?? (object)DBNull.Value);
        cmdPat.Parameters.AddWithValue("Nom", patient.Nom);
        cmdPat.Parameters.AddWithValue("Prenom", patient.Prenom);
        cmdPat.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
        cmdPat.Parameters.AddWithValue("Genre", patient.Genre);
        cmdPat.Parameters.AddWithValue("Adresse", patient.Adresse);
        cmdPat.Parameters.AddWithValue("Telephone", patient.Telephone);
        cmdPat.Parameters.AddWithValue("Mail", patient.Email);
        cmdPat.Parameters.AddWithValue("Id", patient.Id);
        cmdPat.ExecuteNonQuery();
    }
}
