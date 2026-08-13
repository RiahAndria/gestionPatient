using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PaiementService
{
    private readonly string _connectionString;

    public const decimal POURCENTAGE_ACOMPTE_MINIMUM = 0.6m;
    public const int DELAI_ANNULATION_JOURS = 30;
    public const int NB_RELANCES_MAX = 3;

    public PaiementService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public decimal ObtenirTotalAcomptes(NpgsqlConnection conn, string numeroRdv)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT COALESCE(SUM(MONTANT), 0) FROM PAIEMENT WHERE NUMERORDV = @NumeroRdv AND TYPEPAIEMENT = 'ACOMPTE' AND STATUT = true;",
            conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        var resultat = cmd.ExecuteScalar();
        return resultat is null ? 0m : Convert.ToDecimal(resultat);
    }

    public ResultatPaiement EncaisserAcompte(string numeroRdv, decimal montant, string modePaiement)
    {
        if (montant <= 0)
        {
            return new ResultatPaiement
            {
                Succes = false,
                MessageErreur = "Le montant doit être supérieur à zéro.",
                PaiementComplet = false,
                MontantRestant = 0m
            };
        }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryTarif = @"
            SELECT me.TAUX_HORAIRE
            FROM RENDEZ_VOUS r
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            WHERE r.NUMERORDV = @NumeroRdv;";

        using var cmdTarif = new NpgsqlCommand(queryTarif, conn);
        cmdTarif.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        var tarifObj = cmdTarif.ExecuteScalar();

        if (tarifObj is null)
        {
            return new ResultatPaiement
            {
                Succes = false,
                MessageErreur = "Impossible de récupérer le tarif du rendez-vous.",
                PaiementComplet = false,
                MontantRestant = 0m
            };
        }

        decimal tarif = Convert.ToDecimal(tarifObj);
        decimal acomptesActuels = ObtenirTotalAcomptes(conn, numeroRdv);
        decimal resteAvant = Math.Max(0m, tarif - acomptesActuels);

        if (montant > resteAvant)
        {
            return new ResultatPaiement
            {
                Succes = false,
                MessageErreur = $"Le montant dépasse le reste à payer ({resteAvant:N0} Ar).",
                PaiementComplet = false,
                MontantRestant = resteAvant
            };
        }

        string numeroPaiement = $"PAI-{Guid.NewGuid():N}"[..12].ToUpper();

        string insertQuery = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMERORDV, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroRdv, 'ACOMPTE', now(), @Montant, @Mode, true);";

        using (var cmdInsert = new NpgsqlCommand(insertQuery, conn))
        {
            cmdInsert.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
            cmdInsert.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            cmdInsert.Parameters.AddWithValue("Montant", montant);
            cmdInsert.Parameters.AddWithValue("Mode", modePaiement);
            cmdInsert.ExecuteNonQuery();
        }

        decimal nouveauTotalVerse = acomptesActuels + montant;
        decimal montantRestant = Math.Max(0m, tarif - nouveauTotalVerse);

        return new ResultatPaiement
        {
            Succes = true,
            PaiementComplet = montantRestant == 0m,
            MontantRestant = montantRestant
        };
    }

    public (bool EstValide, string? Erreur) ValiderMontantSaisi(decimal montantTotal, decimal montantSaisi, bool estAvance)
    {
        if (montantSaisi <= 0)
        {
            return (false, "Le montant doit être supérieur à zéro.");
        }

        if (estAvance)
        {
            decimal minimum = Math.Round(montantTotal * POURCENTAGE_ACOMPTE_MINIMUM, 0, MidpointRounding.AwayFromZero);
            if (montantSaisi < minimum)
            {
                return (false, $"Le montant doit être au moins {minimum:N0} Ar.");
            }

            if (montantSaisi > montantTotal)
            {
                return (false, $"Le montant ne peut pas dépasser le montant total ({montantTotal:N0} Ar).");
            }

            return (true, null);
        }

        if (montantSaisi != montantTotal)
        {
            return (false, $"Le montant doit être exactement le montant total ({montantTotal:N0} Ar).");
        }

        return (true, null);
    }
}