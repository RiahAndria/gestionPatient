using System;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PatientService
{
    public void AjouterPatient(Patients.Models.Patient patient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            InsererDossierMedical(patient, conn, transaction);
            InsererPersonne(patient, conn, transaction);
            InsererPatient(patient, conn, transaction);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void InsererDossierMedical(Patients.Models.Patient patient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string queryDossier = @"
            INSERT INTO DOSSIER_MEDICAL (NUMERODOSSIER, POIDS, TAILLE, GROUPESANGUIN, ALLERGIES, ANTECEDENTS)
            VALUES (@NumeroDossier, @Poids, @Taille, @GroupeSanguin, @Allergies, @Antecedents);";

        using var cmdDossier = new NpgsqlCommand(queryDossier, conn, transaction);
        cmdDossier.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
        cmdDossier.Parameters.AddWithValue("Poids", 0.0);
        cmdDossier.Parameters.AddWithValue("Taille", 0.0);
        cmdDossier.Parameters.AddWithValue("GroupeSanguin", "N/A");
        cmdDossier.Parameters.AddWithValue("Allergies", DBNull.Value);
        cmdDossier.Parameters.AddWithValue("Antecedents", DBNull.Value);
        cmdDossier.ExecuteNonQuery();
    }

    private static void InsererPersonne(Patients.Models.Patient patient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string queryPersonne = @"
            INSERT INTO PERSONNE (ID, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
            VALUES (@Id, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);";

        using var cmdPersonne = new NpgsqlCommand(queryPersonne, conn, transaction);
        cmdPersonne.Parameters.AddWithValue("Id", patient.Id);
        cmdPersonne.Parameters.AddWithValue("Nom", patient.Nom);
        cmdPersonne.Parameters.AddWithValue("Prenom", patient.Prenom);
        cmdPersonne.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
        cmdPersonne.Parameters.AddWithValue("Genre", patient.Genre);
        cmdPersonne.Parameters.AddWithValue("Adresse", patient.Adresse);
        cmdPersonne.Parameters.AddWithValue("Telephone", patient.Telephone);
        cmdPersonne.Parameters.AddWithValue("Mail", patient.Email);
        cmdPersonne.ExecuteNonQuery();
    }

    private static void InsererPatient(Patients.Models.Patient patient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string queryPatient = @"
            INSERT INTO PATIENT (ID, NUMERODOSSIER, NUMEROASSURANCE, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
            VALUES (@Id, @NumeroDossier, @NumeroAssurance, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);";

        using var cmdPatient = new NpgsqlCommand(queryPatient, conn, transaction);
        cmdPatient.Parameters.AddWithValue("Id", patient.Id);
        cmdPatient.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
        cmdPatient.Parameters.AddWithValue("NumeroAssurance", patient.NumeroAssurance);
        cmdPatient.Parameters.AddWithValue("Nom", patient.Nom);
        cmdPatient.Parameters.AddWithValue("Prenom", patient.Prenom);
        cmdPatient.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
        cmdPatient.Parameters.AddWithValue("Genre", patient.Genre);
        cmdPatient.Parameters.AddWithValue("Adresse", patient.Adresse);
        cmdPatient.Parameters.AddWithValue("Telephone", patient.Telephone);
        cmdPatient.Parameters.AddWithValue("Mail", patient.Email);
        cmdPatient.ExecuteNonQuery();
    }
}
