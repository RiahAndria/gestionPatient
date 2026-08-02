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
    public void AjouterPatient(Patients.Models.Patient patient)
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
                cmdDossier.Parameters.AddWithValue("Poids", 0.0);                   // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("Taille", 0.0);                  // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("GroupeSanguin", "N/A");         // Valeur par défaut
                cmdDossier.Parameters.AddWithValue("Allergies", DBNull.Value);      // Aucun
                cmdDossier.Parameters.AddWithValue("Antecedents", DBNull.Value);    // Inconnu
                cmdDossier.ExecuteNonQuery();
            }

            // et maintenant on insère dans la table personne, svp faites que ça marche sinon je pète un câble
            string queryPersonne = @"
                INSERT INTO PERSONNE (ID, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
                VALUES (@Id, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);"; 

            using (var cmdPersonne = new NpgsqlCommand(queryPersonne, conn, transaction))
            {
                cmdPersonne.Parameters.AddWithValue("Id", patient.Id);                              // Matricule généré par ma fonction là (à vérifier)
                cmdPersonne.Parameters.AddWithValue("Nom", patient.Nom);                            // Nom à vérifier avec un regex
                cmdPersonne.Parameters.AddWithValue("Prenom", patient.Prenom);                      // Idem Nom
                cmdPersonne.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);        // 
                cmdPersonne.Parameters.AddWithValue("Genre", patient.Genre);
                cmdPersonne.Parameters.AddWithValue("Adresse", patient.Adresse);
                cmdPersonne.Parameters.AddWithValue("Telephone", patient.Telephone);
                cmdPersonne.Parameters.AddWithValue("Mail", patient.Email);
                cmdPersonne.ExecuteNonQuery();
            }

            // et maintenant on insère dans la table patient... Je vous ai dit que faire un putain d'héritage était une perte de temps ici
           string queryPatient = @"
                INSERT INTO PATIENT (ID, NUMERODOSSIER, NUMEROASSURANCE, NOM, PRENOM, DATEDENAISSANCE, GENRE, ADRESSE, TELEPHONE, MAIL)
                VALUES (@Id, @NumeroDossier, @NumeroAssurance, @Nom, @Prenom, @DateNaissance, @Genre, @Adresse, @Telephone, @Mail);";
            using (var cmdPatient = new NpgsqlCommand(queryPatient, conn, transaction))
            {
                cmdPatient.Parameters.AddWithValue("Id", patient.Id);
                cmdPatient.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
                cmdPatient.Parameters.AddWithValue("NumeroAssurance", patient.NumeroAssurance);
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
    public List<Patients.Models.Patient> ObtenirTousLesPatients()
    {
        var liste = new List<Patients.Models.Patient>();
        string query = @"
            SELECT p.ID, p.NOM, p.PRENOM, p.DATEDENAISSANCE, p.GENRE, p.ADRESSE, p.TELEPHONE, p.MAIL, 
                pa.NUMERODOSSIER, pa.NUMEROASSURANCE
            FROM PATIENT pa
            INNER JOIN PERSONNE p ON pa.ID = p.ID;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        
        conn.Open();
        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            liste.Add(new Patients.Models.Patient
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
                NumeroAssurance = reader.IsDBNull(9) ? string.Empty : reader.GetString(9) // <--- AJOUT ICI !
            });
        }

        return liste;
    }

    // Mettre à jour les données d'un patient existant
    public void ModifierPatient(Patients.Models.Patient patient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // Mise à jour de la table PERSONNE
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

            // Mise à jour de la table PATIENT
            string updatePatient = @"
                UPDATE PATIENT
                SET NUMERODOSSIER = @NumeroDossier, 
                    NUMEROASSURANCE = @NumeroAssurance,
                    NOM = @Nom, 
                    PRENOM = @Prenom, 
                    DATEDENAISSANCE = @DateNaissance,
                    GENRE = @Genre, 
                    ADRESSE = @Adresse, 
                    TELEPHONE = @Telephone, 
                    MAIL = @Mail
                WHERE ID = @Id;";

            using (var cmdPat = new NpgsqlCommand(updatePatient, conn, transaction))
            {
                cmdPat.Parameters.AddWithValue("NumeroDossier", patient.NumeroDossier);
                cmdPat.Parameters.AddWithValue("NumeroAssurance", patient.NumeroAssurance ?? (object)DBNull.Value); // <--- AJOUT ICI !
                cmdPat.Parameters.AddWithValue("Nom", patient.Nom);
                cmdPat.Parameters.AddWithValue("Prenom", patient.Prenom);
                cmdPat.Parameters.AddWithValue("DateNaissance", patient.DateNaissance);
                cmdPat.Parameters.AddWithValue("Genre", patient.Genre);
                cmdPat.Parameters.AddWithValue("Adresse", patient.Adresse);
                cmdPat.Parameters.AddWithValue("Telephone", patient.Telephone);
                cmdPat.Parameters.AddWithValue("Mail", patient.Email);
                cmdPat.Parameters.AddWithValue("Id", patient.Id);
                cmdPat.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public List<Consultation> ObtenirConsultationsParPatient(string patientId)
    {
        var consultations = new List<Consultation>();

        string query = @"
            SELECT c.NUMEROCONSULTATION, c.DIAGNOSTIQUE, c.NOTESMEDICALES
            FROM CONSULTATION c
            INNER JOIN RENDEZ_VOUS r ON c.NUMERORDV = r.NUMERORDV
            WHERE r.ID = @PatientId
            ORDER BY c.NUMEROCONSULTATION DESC;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PatientId", patientId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            consultations.Add(new Consultation
            {
                NumeroConsultation = reader.GetString(0),
                Diagnostique = reader.GetString(1),
                NotesMedicales = reader.GetString(2)
            });
        }

        return consultations;
    }

    // supprimer un patient de la base de données
    public void SupprimerPatient(string idPatient)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string recupererNumeroDossier = "SELECT NUMERODOSSIER FROM PATIENT WHERE ID = @Id;";
            string numeroDossier = string.Empty;

            using (var cmdRecuperation = new NpgsqlCommand(recupererNumeroDossier, conn, transaction))
            {
                cmdRecuperation.Parameters.AddWithValue("Id", idPatient);
                var resultat = cmdRecuperation.ExecuteScalar();
                numeroDossier = resultat?.ToString() ?? string.Empty;
            }

            string deleteRendezVous = "DELETE FROM RENDEZ_VOUS WHERE ID = @Id;";
            using (var cmdRdv = new NpgsqlCommand(deleteRendezVous, conn, transaction))
            {
                cmdRdv.Parameters.AddWithValue("Id", idPatient);
                cmdRdv.ExecuteNonQuery();
            }

            string deletePatient = "DELETE FROM PATIENT WHERE ID = @Id;";
            using (var cmdPat = new NpgsqlCommand(deletePatient, conn, transaction))
            {
                cmdPat.Parameters.AddWithValue("Id", idPatient);
                cmdPat.ExecuteNonQuery();
            }

            if (!string.IsNullOrWhiteSpace(numeroDossier))
            {
                string deleteDossier = "DELETE FROM DOSSIER_MEDICAL WHERE NUMERODOSSIER = @NumeroDossier;";
                using (var cmdDossier = new NpgsqlCommand(deleteDossier, conn, transaction))
                {
                    cmdDossier.Parameters.AddWithValue("NumeroDossier", numeroDossier);
                    cmdDossier.ExecuteNonQuery();
                }
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