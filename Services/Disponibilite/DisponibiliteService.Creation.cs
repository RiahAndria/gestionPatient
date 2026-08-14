using Npgsql;
using Dapper;
using Patients.Models;

namespace Patients.Services;
//Patients.Services.DisponibiliteService
public partial class DisponibiliteService
{
    public bool CreerDisponibiliteNew(Disponibilite donneMedecin ,List<string> tabNumBloc, DateTime dateSelectionner)
    {
        int i, heure = 0, year, day, month;
        year = dateSelectionner.Year;
        day = dateSelectionner.Day;
        month = dateSelectionner.Month; 

        Temps nouveauTempsDisponible = new Temps();
        nouveauTempsDisponible.id_medecin = donneMedecin.id_medecin;
        nouveauTempsDisponible.date_disponibilite = donneMedecin.date_disponibilite;
         
        //verifier qu'aucune date de disponibilite a une bloque soit creer
        using var connexion = new NpgsqlConnection(_connectionString);
        connexion.Open();
        using var transaction = connexion.BeginTransaction();

        try
        {
            //Creation d'un bloc de disponibilite
            foreach (string block in tabNumBloc)
            {
                nouveauTempsDisponible.numero_bloc = int.Parse(block);
                donneMedecin.numero_bloc = int.Parse(block);

                if (donneMedecin.numero_bloc >= 5 || donneMedecin.numero_bloc <=0 )
                {
                    message = "Valeur impossible";
                    return false;
                }

                var sql = @"INSERT INTO DISPONIBILITE (ID_MEDECIN, DATE_DISPONIBILITE, NUMERO_BLOC) 
                        VALUES (@id_medecin, @date_disponibilite, @numero_bloc);";

                var rowAffected = connexion.Execute(sql,donneMedecin,transaction);
                
                
                //la table disponibilite est creer on va creer  le
                switch (block)
                {
                    case "1":
                        heure = 8;
                        break;
                    case "2":
                        heure = 10;
                        break;
                    case "3":
                        heure = 14;
                        break;
                    case "4":
                        heure = 16;
                        break;
                }

                var requete = @"INSERT INTO TEMPS (id_medecin, DATE_DISPONIBILITE, NUMERO_BLOC, HEURE_DEBUT, HEURE_FIN) 
                                VALUES (@id_medecin, @date_disponibilite, @numero_bloc, @heure_debut, @heure_fin )";
                i= 1;

                DateTime heure_debut = new DateTime(year , month, day, heure , 0, 0);
                DateTime heure_fin = heure_debut.AddMinutes(15);
                nouveauTempsDisponible.heure_debut = heure_debut;
                nouveauTempsDisponible.heure_fin = heure_fin;
                connexion.Execute(requete, nouveauTempsDisponible, transaction);

                i++;
                while (i <= 8)
                {
                    heure_debut = heure_debut.AddMinutes(15);
                    heure_fin = heure_debut.AddMinutes(15);
                    nouveauTempsDisponible.heure_debut = heure_debut;
                    nouveauTempsDisponible.heure_fin = heure_fin;
                    connexion.Execute(requete, nouveauTempsDisponible, transaction);
                    i++;
                }
            }

            transaction.Commit();
            return true;

        } 
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                message = "Vous etes deja disponible a ce moment de la journee";
            }
            return false;
        }
    }

}