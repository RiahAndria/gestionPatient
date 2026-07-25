using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class PatientLookupService
{
    private readonly string _connectionString;

    public PatientLookupService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // terme vide = renvoie tous les patients (utile pour peupler la
    // ComboBox au premier affichage du formulaire).
    public List<PatientOption> Rechercher(string terme)
    {
        var resultat = new List<PatientOption>();

        string query = @"
            SELECT p.ID, p.NOM, p.PRENOM, pa.NUMERODOSSIER
            FROM PATIENT pa
            INNER JOIN PERSONNE p ON pa.ID = p.ID
            WHERE @Terme = '' 
               OR p.NOM ILIKE '%' || @Terme || '%'
               OR p.PRENOM ILIKE '%' || @Terme || '%'
               OR pa.NUMERODOSSIER ILIKE '%' || @Terme || '%'
            ORDER BY p.NOM, p.PRENOM;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Terme", terme ?? "");

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new PatientOption
            {
                Id = reader.GetString(0),
                NomComplet = $"{reader.GetString(1)} {reader.GetString(2)}",
                Matricule = reader.GetString(3)
            });
        }

        return resultat;
    }
}