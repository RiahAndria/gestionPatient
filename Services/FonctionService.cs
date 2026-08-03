

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using Patients.Models;
using Patients.Helpers;
using System.Data.Common;
using System.Transactions;
using System.Windows;
using System.Data.SqlTypes;
using System.Configuration;
using System.Collections.ObjectModel;

namespace Patients.Services;
// Service pour la gestion des fonctions des medecins
public class FonctionService
{
    public string message {get; set; } = string.Empty;
    private string _connectionString;

    public ObservableCollection<Fonction> Fonctions = new ObservableCollection<Fonction>();
    public FonctionService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public bool AjouterNouvelleFonction(Fonction nouvelleFonc)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"INSERT INTO FONCTION (nom_fonction) VALUES (@nom_fonction);";
            var nombreToucher = connection.Execute(sql,nouvelleFonc, transaction);

            transaction.Commit();
            return true;

        }
        catch (PostgresException e)
        {
            transaction.Rollback();
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                message = "Cette fonction figure deja dans la base";
            }
            return false;
        }
    }

    public void recupererLeListeDesFonctionsDynamique()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        try
        {
            var sql = @"SELECT * FROM FONCTION";
            var ListeDesFonction = connection.Query<Fonction>(sql).ToList();
            
            Fonctions.Clear();
            foreach(var chaqueFonction in ListeDesFonction)
            {
                Fonctions.Add(chaqueFonction);
            }
        } catch (NpgsqlException e)
        {
            Console.WriteLine("erreur d'enregistrement de nouvelle fonction: " + e.Message);
            //return new List<Fonction>();
        }
    }

    public List<Fonction> recupererLeListeDesFonctions()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        try
        {
            var sql = @"SELECT * FROM FONCTION";
            var ListeDesFonction = connection.Query<Fonction>(sql).ToList();
            
            return ListeDesFonction;
        } 
        catch (NpgsqlException e)
        {
            Console.WriteLine("erreur d'enregistrement de nouvelle fonction: " + e.Message);
            return new List<Fonction>();
        }
    }
}