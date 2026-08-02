using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Models;
public static class MatriculeHelper
{
    private static readonly object _lock = new();
    private static int _compteurNumerique = 0;
    private static char _lettreCourante = 'A';
    private static bool _etatCharge = false;
    private static string? _connectionString;

    public static string GenererMatricule(string genre, bool estAssure)
    {
        lock (_lock)
        {
            if (!_etatCharge)
            {
                ChargerEtatDepuisBase();
                _etatCharge = true;
            }

            string prefixe = "P";

            // Code Genre : 01 Homme, 02 Femme, 00 Autre
            string codeGenre = genre == "Homme" ? "01" : (genre == "Femme" ? "02" : "00");

            // Code Assurance : 10 Assuré, 00 Non assuré
            string codeAssurance = estAssure ? "10" : "00";

            string codeUnique;
            do
            {
                codeUnique = $"{_compteurNumerique:D3}{_lettreCourante}";
                IncrementeCompteur();
            }
            while (ExisteDeja(codeUnique, codeGenre, codeAssurance));

            return $"{prefixe}-{codeGenre}-{codeAssurance}-{codeUnique}";
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
                InitialiserCompteurDepuisValeurParDefaut();
                return;
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = "SELECT NUMERODOSSIER FROM PATIENT WHERE NUMERODOSSIER IS NOT NULL;";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            int maxNumero = -1;
            char maxLettre = 'A';

            while (reader.Read())
            {
                var matricule = reader.GetString(0);
                if (!TryExtraireCodeUnique(matricule, out int numero, out char lettre))
                {
                    continue;
                }

                if (numero > maxNumero || (numero == maxNumero && lettre > maxLettre))
                {
                    maxNumero = numero;
                    maxLettre = lettre;
                }
            }

            if (maxNumero >= 0)
            {
                _compteurNumerique = maxNumero;
                _lettreCourante = maxLettre;
            }
            else
            {
                InitialiserCompteurDepuisValeurParDefaut();
            }
        }
        catch
        {
            InitialiserCompteurDepuisValeurParDefaut();
        }
    }

    private static void InitialiserCompteurDepuisValeurParDefaut()
    {
        _compteurNumerique = 0;
        _lettreCourante = 'A';
    }

    private static bool TryExtraireCodeUnique(string matricule, out int numero, out char lettre)
    {
        numero = -1;
        lettre = 'A';

        if (string.IsNullOrWhiteSpace(matricule))
        {
            return false;
        }

        string[] parties = matricule.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parties.Length < 4)
        {
            return false;
        }

        string codeUnique = parties[3];
        if (codeUnique.Length < 2)
        {
            return false;
        }

        string numeroPart = codeUnique.Substring(0, Math.Min(3, codeUnique.Length - 1));
        if (!int.TryParse(numeroPart, out numero))
        {
            return false;
        }

        lettre = char.ToUpperInvariant(codeUnique[^1]);
        return lettre >= 'A' && lettre <= 'Z';
    }

    private static bool ExisteDeja(string codeUnique, string codeGenre, string codeAssurance)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return false;
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = "SELECT COUNT(*) FROM PATIENT WHERE NUMERODOSSIER = @matricule;";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@matricule", $"P-{codeGenre}-{codeAssurance}-{codeUnique}");
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void IncrementeCompteur()
    {
        _compteurNumerique++;
        if (_compteurNumerique > 999)
        {
            _compteurNumerique = 0;
            _lettreCourante++;
            if (_lettreCourante > 'Z')
            {
                _lettreCourante = 'A';
            }
        }
    }
}