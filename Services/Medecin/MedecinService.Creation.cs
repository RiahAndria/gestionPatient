using Npgsql;
using Dapper;
using Patients.Models;

namespace Medecins.Services;

public partial class MedecinService
{
    public Boolean AjouterMedecin(Medecin medecin)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
                //INSERTION @ TABLE PERSONNE 
                string sql = @"INSERT INTO personne (id ,nom,prenom ,datedenaissance ,genre ,adresse ,telephone ,mail) 
                            VALUES (@Id, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Email)";
                int rowAffected = connection.Execute(sql, medecin, transaction );

                //INSERTION DANS LE TABLE MEDECIN
                string req = @"INSERT INTO MEDECIN (ID_MEDECIN, NUMERO_ORDRE, STATUT, CODE_FONCTION, TAUX_HORAIRE)
                            VALUES (@Id, @numero_ordre, @statut, @code_fonction, @taux_horaire )";
                int rowTouche = connection.Execute(req, medecin, transaction );

                transaction.Commit();
                return true;

        } 
        catch (PostgresException e)
        {
            transaction.Rollback();
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                message = "Ce numero d'orde appartient deja a une autre personne!";
            }
            return false;
        }
    }
}
