using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

// Recherche legere des medecins, utilisee pour peupler la ComboBox du
// formulaire de creation de rendez-vous.
//
// Le filtre sur la disponibilite (ancienne colonne MEDECIN.DISPONIBILITE,
// supprimee par la refonte du schema) n'existe plus ici : tous les
// medecins sont renvoyes. A terme, ce filtre doit interroger la table
// TEMPS d'Alinot (creneaux de 15 min) pour ne proposer que les medecins
// libres a la date/heure choisie dans le formulaire - a concevoir avec lui.
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

    public List<MedecinOption> ObtenirDisponibles()
    {
        var resultat = new List<MedecinOption>();

        // FONCTION est une table separee depuis la refonte (CODE_FONCTION -> NOM_FONCTION).
        string query = @"
            SELECT m.ID_MEDECIN, p.NOM, p.PRENOM, f.NOM_FONCTION
            FROM MEDECIN m
            INNER JOIN PERSONNE p ON m.ID_MEDECIN = p.ID
            INNER JOIN FONCTION f ON m.CODE_FONCTION = f.CODE_FONCTION
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
