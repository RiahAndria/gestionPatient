using Npgsql;

namespace Patients.Services;

public partial class RendezVousService
{
    // Suppression DEFINITIVE (contrairement a AnnulerRendezVous, qui ne
    // fait que changer le statut). Supprime aussi les donnees liees
    // (notifications, paiements, ordonnances, consultation) avant le
    // rendez-vous lui-meme, dans l'ordre exact impose par les contraintes
    // de cle etrangere du schema (aucune n'est ON DELETE CASCADE vers
    // RENDEZ_VOUS ni CONSULTATION, confirme par l'export SQL fourni) :
    //   NOTIFICATION, PAIEMENT, ORDONANCE (via CONSULTATION), CONSULTATION, RENDEZ_VOUS.
    public void SupprimerDefinitivement(string numeroRdv)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            Executer(conn, transaction, "DELETE FROM NOTIFICATION WHERE NUMERORDV = @NumeroRdv;", numeroRdv);
            Executer(conn, transaction, "DELETE FROM PAIEMENT WHERE NUMERORDV = @NumeroRdv;", numeroRdv);
            Executer(conn, transaction,
                "DELETE FROM ORDONANCE WHERE NUMEROCONSULTATION IN (SELECT NUMEROCONSULTATION FROM CONSULTATION WHERE NUMERORDV = @NumeroRdv);",
                numeroRdv);
            Executer(conn, transaction, "DELETE FROM CONSULTATION WHERE NUMERORDV = @NumeroRdv;", numeroRdv);
            Executer(conn, transaction, "DELETE FROM RENDEZ_VOUS WHERE NUMERORDV = @NumeroRdv;", numeroRdv);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void Executer(NpgsqlConnection conn, NpgsqlTransaction tx, string sql, string numeroRdv)
    {
        using var cmd = new NpgsqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }
}
