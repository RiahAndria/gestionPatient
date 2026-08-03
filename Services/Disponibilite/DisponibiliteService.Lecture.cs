using Npgsql;
using Dapper;
using Patients.Models;

namespace Patients.Services;

public partial class DisponibiliteService
{
    public async Task<List<Temps>> obtenirLesTempsMedecin(DateOnly date, string id_medecin)
    {
        using var connextion = new NpgsqlConnection();
        connextion.Open();
        using var transaction = connextion.BeginTransaction();

        try
        {
            var sql = @"SELECT * FROM TEMPS 
                    WHERE date_disponibilite = @date AND id_medecin = @id_medecin;";
            var resultat =( await connextion.QueryAsync<Temps>(sql,new {date, id_medecin}, transaction)).ToList();
            
            transaction.Commit();
            return resultat;
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            Console.WriteLine("Erreur :" + e.Message);
            return new List<Temps>();
        }
    }
}
