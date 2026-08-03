using Npgsql;

namespace Patients.Services;

public partial class PaiementService
{
    // A appeler par le module Consultation juste apres l'enregistrement
    // d'une consultation : cree la facture en attente pour le solde
    // restant (montant total moins les acomptes deja verses). Si les
    // acomptes couvrent deja tout le tarif, aucune facture n'est creee.
    public ResultatFacture CreerPaiementDu(string numeroConsultation, decimal montantTotal, string modePaiementPropose = "Non précisé")
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string queryRdv = "SELECT NUMERORDV FROM CONSULTATION WHERE NUMEROCONSULTATION = @NumeroConsultation;";
        string numeroRdv;
        using (var cmdRdv = new NpgsqlCommand(queryRdv, conn))
        {
            cmdRdv.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
            var resultat = cmdRdv.ExecuteScalar();
            if (resultat is null)
                throw new InvalidOperationException("Impossible de créer un paiement : la consultation n'existe pas encore (pas de paiement en avance).");
            numeroRdv = (string)resultat;
        }

        decimal acomptesDejaVerses = ObtenirTotalAcomptes(conn, numeroRdv);
        decimal solde = Math.Max(0, montantTotal - acomptesDejaVerses);

        if (solde <= 0)
        {
            return new ResultatFacture
            {
                FactureCreee = false,
                Montant = 0,
                Message = "Ce rendez-vous était déjà entièrement réglé par acompte : aucune facture supplémentaire générée."
            };
        }

        string numeroPaiement = $"PAI-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        string query = @"
            INSERT INTO PAIEMENT (NUMEROPAIEMENT, NUMERORDV, NUMEROCONSULTATION, TYPEPAIEMENT, DATEPAIEMENT, MONTANT, MODEPAIEMENT, STATUT)
            VALUES (@NumeroPaiement, @NumeroRdv, @NumeroConsultation, 'NORMAL', now(), @Montant, @Mode, false);";

        using (var cmd = new NpgsqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("NumeroPaiement", numeroPaiement);
            cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
            cmd.Parameters.AddWithValue("Montant", solde);
            cmd.Parameters.AddWithValue("Mode", modePaiementPropose);
            cmd.ExecuteNonQuery();
        }

        return new ResultatFacture { FactureCreee = true, Montant = solde };
    }

    // Montant total suggere = taux horaire du medecin qui a tenu la
    // consultation (tarif forfaitaire par consultation, faute d'une
    // duree stockee dans le schema). C'est le montant AVANT deduction
    // des acomptes - CreerPaiementDu se charge de la deduction.
    public decimal CalculerMontantSuggere(string numeroConsultation)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            SELECT me.TAUX_HORAIRE
            FROM CONSULTATION c
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            WHERE c.NUMEROCONSULTATION = @NumeroConsultation;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroConsultation", numeroConsultation);
        var resultat = cmd.ExecuteScalar();
        return resultat is decimal montant ? montant : 0m;
    }

    // Meme chose, mais directement depuis un rendez-vous (avant meme
    // qu'une consultation existe) - utilise pour valider un acompte.
    public decimal CalculerMontantSuggereParRdv(string numeroRdv)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        return CalculerMontantSuggereParRdvInterne(conn, numeroRdv);
    }

    private decimal CalculerMontantSuggereParRdvInterne(NpgsqlConnection conn, string numeroRdv)
    {
        string query = @"
            SELECT me.TAUX_HORAIRE
            FROM RENDEZ_VOUS r
            INNER JOIN MEDECIN me ON r.ID_HER_2 = me.ID_MEDECIN
            WHERE r.NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        var resultat = cmd.ExecuteScalar();
        return resultat is decimal montant ? montant : 0m;
    }
}
