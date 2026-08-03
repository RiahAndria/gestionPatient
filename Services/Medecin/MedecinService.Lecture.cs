using Npgsql;
using Dapper;
using Patients.Models;

namespace Medecins.Services;

public partial class MedecinService
{
    public List<Medecin> ObtenirTousLesMedecin()
    {
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
                        p.id AS Id ,
                        p.nom AS Nom , 
                        p.prenom AS Prenom, 
                        p.datedenaissance::timestamp AS DateNaissance, 
                        p.genre AS Genre, 
                        p.adresse AS Adresse, 
                        p.telephone AS Telephone, 
                        p.mail AS Email, 
                        m.numero_ordre AS numero_ordre, 
                        m.statut AS statut, 
                        f.nom_fonction AS nom_fonction, 
                        f.code_fonction AS code_fonction , 
                        m.taux_horaire AS taux_horaire
                    FROM MEDECIN m 	
                    INNER JOIN PERSONNE p ON p.id = m.ID_MEDECIN
                    INNER JOIN FONCTION f ON f.code_fonction = m.CODE_FONCTION
                    WHERE P.ID = @id_medecin;";
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
    // apres un redemarrage de l'application.
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
}
