using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.DirectoryServices;
using Patients.Models;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Patients.Services;

public class DisponibiliteService
{
    private readonly string _connectionString;
    public DisponibiliteService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public bool CreerDisponibilite(Temps nouveauTempsDisponible, int[] tabNumBloc)
    {
        int i, heure = 0;
         
        //verifier qu'aucune date de disponibilite a une bloque soit creer
        using var connexion = new NpgsqlConnection(_connectionString);
        connexion.Open();
        using var transaction = connexion.BeginTransaction();

        try
        {
            //Creation d'un bloc de disponibilite
            foreach (int block in tabNumBloc)
            {
                nouveauTempsDisponible.numero_bloc = block;

                var sql = @"INSERT INTO DISPONIBILITE (ID_MEDECIN, DATE_DISPONIBILITE, NUMERO_BLOC) 
                        VALUES (@id_medecin, @date_disponibilite, @numero_bloc);";

                var rowAffected = connexion.Execute(sql,nouveauTempsDisponible,transaction);

                //la table disponibilite est creer on va creer  le
                switch (block)
                {
                    case 1:
                        heure = 0;
                        break;
                    case 2:
                        heure = 4;
                        break;
                    case 3:
                        heure = 8;
                        break;
                    case 4:
                        heure = 12;
                        break;
                    case 5:
                        heure = 16;
                        break;
                    case 6:
                        heure = 20;
                        break;
                }

                var requete = @"INSERT INTO TEMPS (ID_MEDECIN, DATE_DISPONIBILITE, NUMERO_BLOC, HEURE_DEBUT, HEURE_FIN) 
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
        catch
        {
            transaction.Rollback();
            return false;
        }
    }
}

