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

