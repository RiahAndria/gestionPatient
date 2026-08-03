using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class RendezVousService
{
    // Cree un rendez-vous. Leve une InvalidOperationException si le
    // creneau est deja pris pour ce medecin, ou si la date est deja
    // passee.
    public void AjouterRendezVous(RendezVous rdv)
    {
        if (rdv.DateHeure < DateTime.Now)
        {
            throw new InvalidOperationException("Impossible de créer un rendez-vous dans le passé.");
        }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            if (CreneauDejaPris(conn, transaction, rdv.MedecinID, rdv.DateHeure, exclureNumeroRdv: null))
            {
                throw new InvalidOperationException("Ce médecin a déjà un rendez-vous planifié à ce créneau.");
            }

            string query = @"
                INSERT INTO RENDEZ_VOUS (NUMERORDV, ID, ID_HER_2, DATEHEURERDV, MOTIFRDV, STATUT)
                VALUES (@NumeroRdv, @PatientId, @MedecinId, @DateHeure, @Motif, 'PLANIFIE');";

            using var cmd = new NpgsqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("NumeroRdv", rdv.NumRendezVous);
            cmd.Parameters.AddWithValue("PatientId", rdv.PatientID);
            cmd.Parameters.AddWithValue("MedecinId", rdv.MedecinID);
            cmd.Parameters.AddWithValue("DateHeure", rdv.DateHeure);
            cmd.Parameters.AddWithValue("Motif", rdv.Motif);
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
