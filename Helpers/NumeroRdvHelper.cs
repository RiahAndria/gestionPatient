using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Helpers;

// Genere un numero de rendez-vous sequentiel et lisible (RDV-000001,
// RDV-000002, ...), sur le meme principe que MatriculeHelper (patients)
// et MatriculeHelperMedecin (medecins) : relit l'etat reel depuis la
// base au premier appel de la session, pas de compteur fige en memoire.
//
// Numerotation GLOBALE (toute la clinique), pas par medecin : NUMERORDV
// est la cle primaire de reference unique pour tout le monde
// (secretariat, facturation...), une numerotation par medecin creerait
// des doublons ambigus entre medecins differents.
public static class NumeroRdvHelper
{
    private static readonly object _lock = new();
    private static long _compteur = 0;
    private static bool _etatCharge = false;
    private static string? _connectionString;

    public static string GenererNumero()
    {
        lock (_lock)
        {
            if (!_etatCharge)
            {
                ChargerEtatDepuisBase();
                _etatCharge = true;
            }

            string numero;
            do
            {
                _compteur++;
                numero = $"RDV-{_compteur:D6}";
            }
            while (ExisteDeja(numero));

            return numero;
        }
    }

    private static void ChargerEtatDepuisBase()
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _compteur = 0;
                return;
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = "SELECT NUMERORDV FROM RENDEZ_VOUS WHERE NUMERORDV LIKE 'RDV-%';";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            long max = 0;
            while (reader.Read())
            {
                var num = reader.GetString(0);
                var partieChiffree = num.Length > 4 ? num[4..] : "";
                if (long.TryParse(partieChiffree, out long valeur) && valeur > max)
                {
                    max = valeur;
                }
            }

            _compteur = max;
        }
        catch
        {
            _compteur = 0;
        }
    }

    private static bool ExisteDeja(string numero)
    {
        if (string.IsNullOrWhiteSpace(_connectionString)) return false;

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM RENDEZ_VOUS WHERE NUMERORDV = @num;", conn);
            cmd.Parameters.AddWithValue("@num", numero);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch
        {
            return false;
        }
    }
}
