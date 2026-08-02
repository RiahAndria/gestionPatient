using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Helpers;

public static class MatriculeHelperMedecin
{
    // Meme principe que MatriculeHelper.cs (patients) : plus de
    // compteur fige en memoire qui repart de zero a chaque
    // redemarrage. On relit l'etat reel depuis la base au premier
    // appel de la session, et on verifie l'unicite avant de renvoyer
    // un matricule - important ici car ce matricule devient
    // directement ID_MEDECIN, la cle primaire de la table MEDECIN.
    private static readonly object _lock = new();
    private static int _compteurNumerique = 0;
    private static char _lettreCourante = 'A';
    private static bool _etatCharge = false;
    private static string? _connectionString;

    public static string GenererMatricule(string genre, int code_fonction)
    {
        lock (_lock)
        {
            if (!_etatCharge)
            {
                ChargerEtatDepuisBase();
                _etatCharge = true;
            }

            string prefixe = "M";
            string codeGenre = genre == "Homme" ? "01" : (genre == "Femme" ? "02" : "00");

            string codeUnique;
            do
            {
                codeUnique = $"{_compteurNumerique:D3}{_lettreCourante}";
                IncrementeCompteur();
            }
            while (ExisteDeja(codeUnique));

            return $"{prefixe}-{codeGenre}-{code_fonction}-{codeUnique}";
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

            // ID_MEDECIN suit exactement le meme format que le matricule
            // genere (M-XX-X-XXXA), c'est directement lui qu'on relit.
            const string query = "SELECT ID_MEDECIN FROM MEDECIN WHERE ID_MEDECIN IS NOT NULL;";
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

    private static bool ExisteDeja(string codeUnique)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return false;
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = "SELECT COUNT(*) FROM MEDECIN WHERE ID_MEDECIN LIKE @suffixe;";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@suffixe", $"%-{codeUnique}");

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