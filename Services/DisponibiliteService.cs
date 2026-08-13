using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.DirectoryServices;
using Patients.Models;

// using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Patients.Services;
//Patients.Services.DisponibiliteService
public class DisponibiliteService
{
    private readonly string _connectionString;

    public string message {set; get; } = string.Empty;
    public DisponibiliteService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public bool CreerDisponibilite(Disponibilite donneMedecin ,List<string> tabNumBloc)
    {
        int i, heure = 0;

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
                        heure = 0;
                        break;
                    case "2":
                        heure = 4;
                        break;
                    case "3":
                        heure = 8;
                        break;
                    case "4":
                        heure = 12;
                        break;
                    case "5":
                        heure = 16;
                        break;
                    case "6":
                        heure = 20;
                        break;
                }

                var requete = @"INSERT INTO TEMPS (id_medecin, DATE_DISPONIBILITE, NUMERO_BLOC, HEURE_DEBUT, HEURE_FIN) 
                                VALUES (@id_medecin, @date_disponibilite, @numero_bloc, @heure_debut, @heure_fin )";
                i= 1;

                DateTime heure_debut = new DateTime(2026, 3, 31, heure , 0, 0);
                DateTime heure_fin = heure_debut.AddMinutes(15);
                nouveauTempsDisponible.heure_debut = heure_debut;
                nouveauTempsDisponible.heure_fin = heure_fin;
                connexion.Execute(requete, nouveauTempsDisponible, transaction);

                i++;
                while (i <= 16)
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


    public List<Disponibilite> obtenir_Block_de_temps_disponible_un_jour(DateOnly date, string id_medecin)
    {
        using var connextion = new NpgsqlConnection();
        connextion.Open();
        using var transaction = connextion.BeginTransaction();

        try
        {
            var sql = @"SELECT * FROM TEMPS 
                    WHERE date_disponibilite = @date AND id_medecin = @id_medecin;";
            var resultat =( connextion.Query<Disponibilite>(sql,new {date, id_medecin}, transaction)).ToList();
            
            transaction.Commit();
            return resultat;
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            Console.WriteLine("Erreur :" + e.Message);
            return new List<Disponibilite>();
        }
    }

    public List<Temps> obtenir_etat_de_chaque_15(DateOnly date, string id_medecin, int numero_bloc)
    {
        using var connection =  new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"SELECT * FROM temps 
                        WHERE numero_bloc =:numero_bloc
                        id_medecin =:id_medecin
                        date_disponibilite =:date ";
            var Etat_Chaque_bloc = connection.Query<Temps>(sql, new {numero_bloc, id_medecin, date}).ToList();
            return Etat_Chaque_bloc;

        } catch (NpgsqlException e)
        {
            message = "Erreur lors du obtenir_etat_de_chaque_15" + e.Message;
            return new List<Temps>();
        }
    }

    /**
        Maintenant je vais creer une fonction qui a pour role 
        de tra    */
    public async Task<AgendaJournee> ObtenirAgendaJourneeAsync(string id_medecin, DateTime date)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT 
                    id_temps, 
                    id_medecin, 
                    date_disponibilite::timestamp AS date_disponibilite,
                    numero_bloc,
                    heure_debut,
                    heure_fin,
                    est_disponible,
                    est_reserve 
                    FROM temps
                    WHERE id_medecin =:id_medecin 
                    AND date_disponibilite =:date
                    ORDER BY numero_bloc, heure_debut;
                ";
        try
        {
            var resultat = (await connection.QueryAsync<Temps>(sql, new {id_medecin, date.Date }));
            return new AgendaJournee {
                Date = date,
                Id_medecin = id_medecin,
                Creneaux15Min = resultat.ToList()            
            };

        } 
        catch (NpgsqlException e)
        {
            message = "message lors de l'appel de ObtenirAgendaJourneeAsync : " + e.Message;
            return new AgendaJournee();
        }
    }

    public async Task<List<AgendaJournee>> ObtenirAgendaUneSemaine(string id_medecin, DateTime date)
    {
        var AgendaSemaine = new List<AgendaJournee>();

        DateTime dateDebut = date.Date;
        for (int i = 0; i < 6; i++)
        {
            DateTime DateCourant = dateDebut.AddDays(i);
            var Agenda_journee = await ObtenirAgendaJourneeAsync(id_medecin, DateCourant);
            AgendaSemaine.Add(Agenda_journee);
            
        }
        return AgendaSemaine;
    }
}

