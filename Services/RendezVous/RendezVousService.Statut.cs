using Npgsql;

namespace Patients.Services;

public partial class RendezVousService
{
    // Change librement le statut d'un rendez-vous (bouton "Changer
    // Statut", couleur jaune, dans la fenetre de detail). Contrairement
    // a AnnulerRendezVous (qui exige un motif), ce changement est libre
    // entre les 3 valeurs possibles : PLANIFIE / TERMINE / ANNULE.
    public void ChangerStatut(string numeroRdv, string nouveauStatut)
    {
        if (nouveauStatut is not ("PLANIFIE" or "TERMINE" or "ANNULE"))
        {
            throw new ArgumentException($"Statut inconnu : {nouveauStatut}");
        }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string query = @"
            UPDATE RENDEZ_VOUS
            SET STATUT = @Statut
            WHERE NUMERORDV = @NumeroRdv;";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("Statut", nouveauStatut);
        cmd.Parameters.AddWithValue("NumeroRdv", numeroRdv);
        cmd.ExecuteNonQuery();
    }
}
