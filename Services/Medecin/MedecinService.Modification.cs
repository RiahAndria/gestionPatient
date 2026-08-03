using Npgsql;
using Dapper;
using Patients.Models;

namespace Medecins.Services;

public partial class MedecinService
{
    public bool ModificationMedecin(Medecin donneeMedecinMiseAJour)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            //Mise a jour de personne 
            var sql = @"UPDATE Personne SET
                        nom = @Nom,
                        prenom = @Prenom,
                        datedenaissance = @DateNaissance,
                        genre = @Genre,
                        adresse = @Adresse,
                        telephone = @Telephone,
                        mail = @Email
                        WHERE id= @Id;";
            conn.Execute(sql, donneeMedecinMiseAJour, transaction);

            //Mise a jour de table medecin
            var sqlMed = @"UPDATE MEDECIN SET
                        NUMERO_ORDRE = @numero_ordre, 
                        STATUT = @statut, 
                        CODE_FONCTION = @code_fonction, 
                        TAUX_HORAIRE = @taux_horaire
                        WHERE ID_MEDECIN = @Id;
                        ";
            conn.Execute(sqlMed, donneeMedecinMiseAJour, transaction);
            
            transaction.Commit();
            return true;
        } 
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine("Erreur lors de M.A.J de medecin : " + e.Message);
            return false;
        }
    }
}
