using Npgsql;

namespace Patients.Services;

public partial class RendezVousService
{
    // La ligne n'est jamais supprimee (on garde l'historique) : on
    // passe simplement le statut a ANNULE avec le motif fourni.
    public void AnnulerRendezVous(string numeroRdv, string motif)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE RENDEZ_VOUS
            SET STATUT = 'ANNULE', MOTIFANNULATION = @Motif
            WHERE NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Motif", motif);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }

    // Change la date/heure d'un rendez-vous, en revalidant qu'aucun
    // conflit de creneau n'est cree. Refuse si une consultation est deja
    // rattachee (contrainte UNIQUE CONSULTATION.NUMERORDV) : il faut
    // alors annuler puis creer un nouveau rendez-vous.
    public void ReprogrammerRendezVous(string numeroRdv, DateTime nouvelleDateHeure)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string queryVerifConsultation = "SELECT COUNT(*) FROM CONSULTATION WHERE NUMERORDV = @NumeroRdv;";
            using (var cmdVerif = new NpgsqlCommand(queryVerifConsultation, conn, transaction))
            {
                cmdVerif.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                var nbConsultations = (long)cmdVerif.ExecuteScalar()!;
                if (nbConsultations > 0)
                {
                    throw new InvalidOperationException(
                        "Ce rendez-vous a déjà une consultation associée : annule-le et crée un nouveau rendez-vous plutôt que de le reprogrammer.");
                }
            }

            string queryMedecin = "SELECT ID_HER_2 FROM RENDEZ_VOUS WHERE NUMERORDV = @NumeroRdv;";
            string medecinId;
            using (var cmdMedecin = new NpgsqlCommand(queryMedecin, conn, transaction))
            {
                cmdMedecin.Parameters.AddWithValue("NumeroRdv", numeroRdv);
                medecinId = (string)(cmdMedecin.ExecuteScalar() ?? throw new InvalidOperationException("Rendez-vous introuvable."));
            }

            if (CreneauDejaPris(conn, transaction, medecinId, nouvelleDateHeure, exclureNumeroRdv: numeroRdv))
            {
                throw new InvalidOperationException("Ce médecin a déjà un rendez-vous planifié sur ce nouveau créneau.");
            }

            string queryUpdate = "UPDATE RENDEZ_VOUS SET DATEHEURERDV = @NouvelleDate WHERE NUMERORDV = @NumeroRdv;";
            using var cmdUpdate = new NpgsqlCommand(queryUpdate, conn, transaction);
            cmdUpdate.Parameters.AddWithValue("NouvelleDate", nouvelleDateHeure);
            cmdUpdate.Parameters.AddWithValue("NumeroRdv", numeroRdv);
            cmdUpdate.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
