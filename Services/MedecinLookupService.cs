using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class MedecinLookupService
{
    private readonly string _connectionString;

    public MedecinLookupService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // Ne renvoie que les medecins marques disponibles (DISPONIBILITE = true).
    // Note : ceci ne verifie PAS les conflits de creneau, seulement la disponibilite generale declaree par Alinot dans son module.
    public List<MedecinOption> ObtenirDisponibles()
    {
        var resultat = new List<MedecinOption>();

        string query = @"
            SELECT m.ID_HER_2, p.NOM, p.PRENOM, m.FONCTION
            FROM MEDECIN m
            INNER JOIN PERSONNE p ON m.ID_HER_2 = p.ID
            WHERE m.DISPONIBILITE = true
            ORDER BY p.NOM, p.PRENOM;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            resultat.Add(new MedecinOption
            {
                IdHer2 = reader.GetString(0),
                NomComplet = $"Dr. {reader.GetString(1)} {reader.GetString(2)}",
                Fonction = reader.GetString(3)
            });
        }

        return resultat;
    }
}