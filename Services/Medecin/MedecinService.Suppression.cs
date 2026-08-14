using Npgsql;
using Dapper;

namespace Medecins.Services;

public partial class MedecinService
{
    public bool SupprimerMedecin(string Id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            //Suppression dans le table medecin
            var sql = "DELETE FROM Medecin WHERE ID_MEDECIN = @Id";
            conn.Execute(sql, new {Id}, transaction);

            //suppression de la table personne
            var req = "DELETE FROM PERSONNE WHERE ID = @Id";
            conn.Execute(req, new {Id}, transaction);
            
            transaction.Commit();
            return true;
        } 
        catch (Exception e)
        {
            transaction.Rollback();
            message = "Erreur de suppression de Medecin" + e.Message;
            return false;
        }
        
    }
}
