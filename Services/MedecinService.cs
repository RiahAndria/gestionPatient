using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using Patients.Models;
using Patients.Helpers;
using System.Data.Common;
using System.Transactions;

namespace Medecins.Services;

public class MedecinService
{
    private readonly string _connectionString;

    public MedecinService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

       _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

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
        catch (NpgsqlException)
        {
            transaction.Rollback();
            return false;
        }
    }


    public List<Medecin> ObtenirTousLesMedecin()
    {
        // List<Medecin> ListeMedecin = new List<Medecin>();
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var sql = @"SELECT p.ID, p.NOM, p.PRENOM, p.DATEDENAISSANCE, p.GENRE, p.ADRESSE, p.TELEPHONE, p.MAIL AS EMAIL, m.NUMERO_ORDRE, m.STATUT, f.NOM_FONCTION, f.CODE_FONCTION , m.TAUX_HORAIRE
                    FROM MEDECIN m 	
                    INNER JOIN PERSONNE p ON p.ID = m.ID_MEDECIN
                    INNER JOIN FONCTION f ON f.CODE_FONCTION = m.CODE_FONCTION;";

            var ListeMedecin = conn.Query<Medecin>(sql,transaction).ToList(); 

            return ListeMedecin;
        } 
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Erreur obtention Liste de medecin: {ex.Message}");
            return new List<Medecin>();
        }
    }


    public Medecin ObtenirDonnePersonnelMedecin(string id_medecin)
    {
        using var connexion = new NpgsqlConnection(_connectionString);
        connexion.Open();
        using var transaction = connexion.BeginTransaction();

        try
        {
            var sql = @"SELECT 
                        p.ID AS Id ,
                        p.NOM AS Nom , 
                        p.PRENOM AS Prenom, 
                        p.DATEDENAISSANCE AS DateNaissance, 
                        p.GENRE AS Genre, 
                        p.ADRESSE AS Adresse, 
                        p.TELEPHONE AS Telephone, 
                        p.MAIL AS Email, 
                        m.NUMERO_ORDRE AS numero_ordre, 
                        m.STATUT AS statut, 
                        f.NOM_FONCTION AS nom_fonction, 
                        f.CODE_FONCTION AS code_fonction , 
                        m.TAUX_HORAIRE AS taux_horaire
                    FROM MEDECIN m 	
                    INNER JOIN PERSONNE p ON p.ID = m.ID_MEDECIN
                    INNER JOIN FONCTION f ON f.CODE_FONCTION = m.CODE_FONCTION
                    WHERE P.ID = @id_medecin;";
                    //WHERE P.ID = 'M-02-1-000A';";
            var donnees = connexion.QueryFirstOrDefault<Medecin>(sql, new {id_medecin});
            var valeur = (donnees == null ) ? new Medecin() : donnees;
            Console.Write(valeur.DateNaissance);
            return valeur;               

        } 
        catch
        {
            return new Medecin();
        }
    }

    public int RecupererCodeFonction(string nom_fonc)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        try
        {
            var sql = "SELECT CODE_FONCTION FROM FONCTION WHERE NOM_FONCTION = @nom";
            using (var execute = new NpgsqlCommand(sql, conn))
            {
                execute.Parameters.AddWithValue("nom", nom_fonc);
                var resultat = execute.ExecuteScalar(); 

                int value = (resultat == null || resultat == DBNull.Value) ? 0 : Convert.ToInt32(resultat);
                return value;
            }
        } catch {
            return 0;
        }
    }

    // Le numero d'ordre des medecins est UNIQUE en base (contrainte
    // medecin_numero_ordre_key). On le verifie AVANT l'insertion pour
    // afficher un message clair au lieu d'une erreur SQL generique.
    public bool NumeroOrdreExiste(string numeroOrdre)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        string sql = "SELECT COUNT(*) FROM MEDECIN WHERE NUMERO_ORDRE = @numeroOrdre;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("numeroOrdre", numeroOrdre);

        return Convert.ToInt64(cmd.ExecuteScalar()!) > 0;
    }

    // Dernier compteur numerique deja utilise dans les matricules
    // medicaux (format M-XX-YY-000A) : sert a continuer la numerotation
    // apres un redemarrage de l'application, sinon le compteur statique
    // repart de zero et les nouveaux id_medecin entrent en collision
    // avec des medecins deja enregistres (cle primaire).
    public int ObtenirDernierCompteurMatricule()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var ids = conn.Query<string>(
            "SELECT ID_MEDECIN FROM MEDECIN WHERE ID_MEDECIN LIKE 'M-%';").ToList();

        int max = 0;
        foreach (var id in ids)
        {
            var parties = id.Split('-');
            if (parties.Length == 4 && parties[3].Length >= 3
                && int.TryParse(parties[3][..3], out int compteur))
            {
                max = Math.Max(max, compteur);
            }
        }
        return max;
    }

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

    public bool SupprimerMedecin(string Id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            //Suppression dans le table medecin
            var sql = "DELETE FROM Medecin WHERE ID_MEDECIN = @Id";
            conn.Execute(sql, new {Id}, transaction);

            //suppression de la table personne
            var req = "DELETE FROM PERSONNE WHERE ID = @Id";
            conn.Execute(req, new {Id}, transaction);
            
            transaction.Commit();
            return true;
        } 
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine("Erreur de suppression de Medecin" + e.Message);
            return false;
        }
        
    }
}
