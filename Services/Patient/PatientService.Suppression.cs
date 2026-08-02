using System;
using Npgsql;

namespace Patients.Services;

public partial class PatientService
{
    public void SupprimerPatient(string idPatient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var numeroDossier = RecupererNumeroDossier(idPatient, conn, transaction);
            SupprimerRendezVous(idPatient, conn, transaction);
            SupprimerPatientPrincipal(idPatient, conn, transaction);

            if (!string.IsNullOrWhiteSpace(numeroDossier))
            {
                SupprimerDossierMedical(numeroDossier, conn, transaction);
            }

            SupprimerPersonne(idPatient, conn, transaction);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private static string RecupererNumeroDossier(string idPatient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string recupererNumeroDossier = "SELECT NUMERODOSSIER FROM PATIENT WHERE ID = @Id;";
        using var cmdRecuperation = new NpgsqlCommand(recupererNumeroDossier, conn, transaction);
        cmdRecuperation.Parameters.AddWithValue("Id", idPatient);
        var resultat = cmdRecuperation.ExecuteScalar();
        return resultat?.ToString() ?? string.Empty;
    }

    private static void SupprimerRendezVous(string idPatient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string deleteRendezVous = "DELETE FROM RENDEZ_VOUS WHERE ID = @Id;";
        using var cmdRdv = new NpgsqlCommand(deleteRendezVous, conn, transaction);
        cmdRdv.Parameters.AddWithValue("Id", idPatient);
        cmdRdv.ExecuteNonQuery();
    }

    private static void SupprimerPatientPrincipal(string idPatient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string deletePatient = "DELETE FROM PATIENT WHERE ID = @Id;";
        using var cmdPat = new NpgsqlCommand(deletePatient, conn, transaction);
        cmdPat.Parameters.AddWithValue("Id", idPatient);
        cmdPat.ExecuteNonQuery();
    }

    private static void SupprimerDossierMedical(string numeroDossier, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string deleteDossier = "DELETE FROM DOSSIER_MEDICAL WHERE NUMERODOSSIER = @NumeroDossier;";
        using var cmdDossier = new NpgsqlCommand(deleteDossier, conn, transaction);
        cmdDossier.Parameters.AddWithValue("NumeroDossier", numeroDossier);
        cmdDossier.ExecuteNonQuery();
    }

    private static void SupprimerPersonne(string idPatient, NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        const string deletePersonne = "DELETE FROM PERSONNE WHERE ID = @Id;";
        using var cmdPers = new NpgsqlCommand(deletePersonne, conn, transaction);
        cmdPers.Parameters.AddWithValue("Id", idPatient);
        cmdPers.ExecuteNonQuery();
    }
}
