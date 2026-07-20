using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public class PatientService
{
    
    private readonly string _connectionString;
    public PatientService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
    public void AjouterPatient(Patient patient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            // Création du dossier medical vu que cet enfoiré de patient veut pas se créer si le dossier existe pas encore
            string queryDossier = @"
                INSERT INTO DOSSIER_MEDICAL (NUMERODOSSIER, POIDS, TAILLE, GROUPESANGUIN, ALLERGIES, ANTECEDENTS)
                VALUES (@NumeroDossier, @Poids, @Taille, @GroupeSanguin, @Allergies, @Antecedents);";

            using (var cmdDossier = new NpgsqlCommand(queryDossier, conn, transaction))
            {
                cmdDossier.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
                cmdDossier.Parameters.AddWithValue("Poids", 0.0);           // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("Taille", 0.0);          // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("GroupeSanguin", "N/A"); // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("Allergies", DBNull.Value);
                cmdDossier.Parameters.AddWithValue("Antecedents", DBNull.Value);
                cmdDossier.ExecuteNonQuery();
            }

            // et maintenant on insère dans la table personne, svp faites que ça marche sinon je pète un câble
            string queryPersonne = @"
                INSERT INTO PERSONNE (ID, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
                VALUES (@Id, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);";

            using (var cmdPersonne = new NpgsqlCommand(queryPersonne, conn, transaction))
            {
                cmdPersonne.Parameters.AddWithValue("Id", patient.Id);
                cmdPersonne.Parameters.AddWithValue("Nom", patient.Nom);
                cmdPersonne.Parameters.AddWithValue("Prenom", patient.Prenom);
                cmdPersonne.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
                cmdPersonne.Parameters.AddWithValue("Genre", patient.Genre);
                cmdPersonne.Parameters.AddWithValue("Adresse", patient.Adresse);
                cmdPersonne.Parameters.AddWithValue("Telephone", patient.Telephone);
                cmdPersonne.Parameters.AddWithValue("Mail", patient.Email);
                cmdPersonne.ExecuteNonQuery();
            }

            // et maintenant on insère dans la table patient... Je vous ai dit que faire un putain d'héritage était une perte de temps ici
            string queryPatient = @"
                INSERT INTO PATIENT (ID, NUMERODOSSIER, NUMEROPRESCRITPTION, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
                VALUES (@Id, @NumeroDossier, @NumeroPrescription, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);";

            using (var cmdPatient = new NpgsqlCommand(queryPatient, conn, transaction))
            {
                cmdPatient.Parameters.AddWithValue("Id", patient.Id);
                cmdPatient.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
                cmdPatient.Parameters.AddWithValue("NumeroPrescription", string.IsNullOrEmpty(patient.NumeroPrescritption) ? (object)DBNull.Value : patient.NumeroPrescritption);
                cmdPatient.Parameters.AddWithValue("Nom", patient.Nom);
                cmdPatient.Parameters.AddWithValue("Prenom", patient.Prenom);
                cmdPatient.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
                cmdPatient.Parameters.AddWithValue("Genre", patient.Genre);
                cmdPatient.Parameters.AddWithValue("Adresse", patient.Adresse);
                cmdPatient.Parameters.AddWithValue("Telephone", patient.Telephone);
                cmdPatient.Parameters.AddWithValue("Mail", patient.Email);
                
                cmdPatient.ExecuteNonQuery();
            }

            // Valider l'ensemble si tout s'est bien passé
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw; 
        }
    }

    // Récupérer la liste complète de tous les patients
    public List<Patient> ObtenirTousLesPatients()
    {
        var liste = new List<Patient>();
        string query = @"
            SELECT p.ID, p.NOM, p.PRENOM, p.DATEDENAISSANCE, p.GENRE, p.ADRESSE, p.TELEPHONE, p.MAIL, 
                   pa.NUMERODOSSIER, pa.NUMEROPRESCRITPTION
            FROM PATIENT pa
            INNER JOIN PERSONNE p ON pa.ID = p.ID;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        
        conn.Open();
        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            liste.Add(new Patient
            {
                Id = reader.GetString(0),
                Nom = reader.GetString(1),
                Prenom = reader.GetString(2),
                DateNaissance = reader.GetDateTime(3),
                Genre = reader.GetString(4),
                Adresse = reader.GetString(5),
                Telephone = reader.GetString(6),
                Email = reader.GetString(7),
                NumeroDossier = reader.GetString(8),
                // Sécurisation contre le NULL :
                NumeroPrescritption = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            });
        }

        return liste;
    }

    // Mettre à jour les données d'un patient existant
    public void ModifierPatient(Patient patient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            //Update de la table personne
            string updatePersonne = @"
                UPDATE PERSONNE 
                SET NOM = @Nom, PRENOM = @Prenom, DATEDENAISSANCE = @DateNaissance, 
                    GENRE = @Genre, ADRESSE = @Adresse, TELEPHONE = @Telephone, MAIL = @Mail
                WHERE ID = @Id;";

            using (var cmdPers = new NpgsqlCommand(updatePersonne, conn, transaction))
            {
                cmdPers.Parameters.AddWithValue("Nom", patient.Nom);
                cmdPers.Parameters.AddWithValue("Prenom", patient.Prenom);
                cmdPers.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
                cmdPers.Parameters.AddWithValue("Genre", patient.Genre);
                cmdPers.Parameters.AddWithValue("Adresse", patient.Adresse);
                cmdPers.Parameters.AddWithValue("Telephone", patient.Telephone);
                cmdPers.Parameters.AddWithValue("Mail", patient.Email);
                cmdPers.Parameters.AddWithValue("Id", patient.Id);
                cmdPers.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    // supprimer un patient de la base de données
    public void SupprimerPatient(string idPatient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // On supprime d'adord patient puis on supprime personne... Bordel d'héritage de merde...
            string deletePatient = "DELETE FROM PATIENT WHERE ID = @Id;";
            using (var cmdPat = new NpgsqlCommand(deletePatient, conn, transaction))
            {
                cmdPat.Parameters.AddWithValue("Id", idPatient);
                cmdPat.ExecuteNonQuery();
            }

            string deletePersonne = "DELETE FROM PERSONNE WHERE ID = @Id;";
            using (var cmdPers = new NpgsqlCommand(deletePersonne, conn, transaction))
            {
                cmdPers.Parameters.AddWithValue("Id", idPatient);
                cmdPers.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
} // je vous déteste d'avoir insisté pour faire cet héritage de merde là... T T