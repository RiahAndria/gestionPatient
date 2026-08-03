using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Services;

// Service RendezVous, fractionne en plusieurs fichiers :
//   - RendezVousService.Lecture.cs      : Rechercher, ObtenirTous, ObtenirDetail
//   - RendezVousService.Creation.cs     : AjouterRendezVous
//   - RendezVousService.Modification.cs : AnnulerRendezVous, ReprogrammerRendezVous
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

    // Regle metier : un medecin ne peut pas avoir deux rendez-vous
    // planifies au meme instant. Utilise par Creation et Modification,
    // d'ou sa presence ici dans le fichier de base.
    private bool CreneauDejaPris(NpgsqlConnection conn, NpgsqlTransaction tx, string medecinId, DateTime dateHeure, string? exclureNumeroRdv)
    {
        string query = @"
            SELECT COUNT(*) FROM RENDEZ_VOUS
            WHERE ID_HER_2 = @MedecinId
              AND DATEHEURERDV = @DateHeure
              AND STATUT = 'PLANIFIE'
              AND (@ExclureNumero = '' OR NUMERORDV <> @ExclureNumero);";

        using var cmd = new NpgsqlCommand(query, conn, tx);
        cmd.Parameters.AddWithValue("MedecinId", medecinId);
        cmd.Parameters.AddWithValue("DateHeure", dateHeure);
        cmd.Parameters.AddWithValue("ExclureNumero", exclureNumeroRdv ?? "");

        var count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }
}
