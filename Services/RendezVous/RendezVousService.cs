using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Services;

public partial class RendezVousService
{
    private readonly string _connectionString;

    public RendezVousService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public void AjouterRendezVous(Patients.Models.RendezVous rendezVous)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            INSERT INTO RENDEZ_VOUS (NUMERORDV, ID, ID_HER_2, DATEHEURERDV, MOTIFRDV, STATUT, MOTIFANNULATION)
            VALUES (@NumeroRdv, @PatientId, @MedecinId, @DateHeure, @Motif, @Statut, @MotifAnnulation);";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", rendezVous.NumRendezVous);
        cmd.Parameters.AddWithValue("PatientId", rendezVous.PatientID);
        cmd.Parameters.AddWithValue("MedecinId", rendezVous.MedecinID);
        cmd.Parameters.AddWithValue("DateHeure", rendezVous.DateHeure);
        cmd.Parameters.AddWithValue("Motif", rendezVous.Motif);
        cmd.Parameters.AddWithValue("Statut", rendezVous.Statut);
        cmd.Parameters.AddWithValue("MotifAnnulation", (object?)rendezVous.MotifAnnulation ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void AnnulerRendezVous(string numeroRdv, string motif)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE RENDEZ_VOUS
            SET STATUT = 'ANNULE', MOTIFANNULATION = @Motif
            WHERE NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Motif", motif);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }

    public void ReprogrammerRendezVous(string numeroRdv, DateTime nouvelleDateHeure)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE RENDEZ_VOUS
            SET DATEHEURERDV = @DateHeure
            WHERE NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("DateHeure", nouvelleDateHeure);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }
}
