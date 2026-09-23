using Npgsql;
using Patients.Models;

namespace Patients.Services;

// Recherches utilisees par l'assistant de creation de rendez-vous
// (etape 4 : "Creneau horaire + Medecin").
//
// REGLE METIER (demandee explicitement pour ce flux) : un medecin est
// TOUJOURS considere disponible pour n'importe quelle date/creneau
// futur, SAUF s'il a deja un rendez-vous PLANIFIE exactement a cette
// date et heure. On ne depend donc plus des tables DISPONIBILITE/TEMPS
// (systeme de creneaux de 15 min pre-saisis, qui suppose une saisie
// manuelle prealable non utilisee en pratique) : la disponibilite est
// calculee a la volee directement contre RENDEZ_VOUS, avec exactement
// la meme condition que RendezVousService.CreneauDejaPris (ID_HER_2 +
// DATEHEURERDV + STATUT = 'PLANIFIE'), pour que l'etape 4 et la
// creation reelle du rendez-vous (etape 5) restent toujours coherentes.
//
// On propose 3 creneaux fixes par jour, correspondant aux horaires
// d'ouverture (08h00-18h00) : Matin / Apres-midi / Fin de journee. Le
// rendez-vous est cree a l'heure de DEBUT du creneau choisi (voir
// AssistantRendezVousState.DateHeureRdv).
public partial class DisponibiliteService
{
    private static readonly List<CreneauBloc> _blocsProposes = new()
    {
        new CreneauBloc { NumeroBloc = 3, Libelle = "Matin (08h00 - 12h00)", HeureDebut = TimeSpan.FromHours(8), HeureFin = TimeSpan.FromHours(12) },
        new CreneauBloc { NumeroBloc = 4, Libelle = "Après-midi (12h00 - 16h00)", HeureDebut = TimeSpan.FromHours(12), HeureFin = TimeSpan.FromHours(16) },
        new CreneauBloc { NumeroBloc = 5, Libelle = "Fin de journée (16h00 - 18h00)", HeureDebut = TimeSpan.FromHours(16), HeureFin = TimeSpan.FromHours(18) },
    };

    // Blocs (Matin/Apres-midi/Fin de journee) pour lesquels au moins un
    // medecin du service (code_fonction) donne est encore libre a cette
    // date (et si la date est aujourd'hui, dont l'heure de debut n'est
    // pas deja passee).
    public List<CreneauBloc> ObtenirBlocsDisponibles(DateOnly date, int codeFonction)
    {
        var resultat = new List<CreneauBloc>();
        var maintenant = DateTime.Now;

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        foreach (var bloc in _blocsProposes)
        {
            DateTime dateHeureBloc = date.ToDateTime(TimeOnly.FromTimeSpan(bloc.HeureDebut));
            if (dateHeureBloc < maintenant) continue; // creneau deja passe aujourd'hui

            if (AuMoinsUnMedecinLibre(conn, codeFonction, dateHeureBloc))
            {
                resultat.Add(bloc);
            }
        }

        return resultat;
    }

    // Medecins du service donne encore libres sur CE bloc, a cette date
    // (aucun rendez-vous PLANIFIE deja enregistre a cette date/heure
    // exacte pour eux).
    public List<MedecinDisponible> ObtenirMedecinsDisponibles(DateOnly date, int codeFonction, int numeroBloc)
    {
        var bloc = _blocsProposes.Find(b => b.NumeroBloc == numeroBloc);
        if (bloc is null) return new List<MedecinDisponible>();

        DateTime dateHeureBloc = date.ToDateTime(TimeOnly.FromTimeSpan(bloc.HeureDebut));
        var resultat = new List<MedecinDisponible>();

        var codesFonctions = ObtenirCodesFonctionsCompatibles(codeFonction);

        if (codesFonctions.Count == 0)
        {
            codesFonctions = new List<int> { codeFonction };
        }

        string placeholders = string.Join(", ", codesFonctions.Select((_, index) => $"@CodeFonction{index}"));

        string query = $@"
            SELECT m.ID_MEDECIN, p.NOM, p.PRENOM, f.NOM_FONCTION, m.TAUX_HORAIRE
            FROM MEDECIN m
            INNER JOIN PERSONNE p ON p.ID = m.ID_MEDECIN
            INNER JOIN FONCTION f ON f.CODE_FONCTION = m.CODE_FONCTION
            WHERE m.CODE_FONCTION IN ({placeholders})
              AND LOWER(TRIM(m.STATUT)) = 'actif'
              AND NOT EXISTS (
                  SELECT 1 FROM RENDEZ_VOUS r
                  WHERE r.ID_HER_2 = m.ID_MEDECIN
                    AND r.DATEHEURERDV = @DateHeure
                    AND r.STATUT = 'PLANIFIE'
              )
            ORDER BY p.NOM, p.PRENOM;";

        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(query, conn);
        for (int i = 0; i < codesFonctions.Count; i++)
        {
            cmd.Parameters.AddWithValue($"CodeFonction{i}", codesFonctions[i]);
        }
        cmd.Parameters.AddWithValue("DateHeure", dateHeureBloc);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            resultat.Add(new MedecinDisponible
            {
                Id = reader.GetString(0),
                NomComplet = $"Dr. {reader.GetString(1)} {reader.GetString(2)}",
                Fonction = reader.GetString(3),
                TauxHoraire = reader.GetDecimal(4)
            });
        }

        return resultat;
    }

    private bool AuMoinsUnMedecinLibre(NpgsqlConnection conn, int codeFonction, DateTime dateHeureBloc)
    {
        var codesFonctions = ObtenirCodesFonctionsCompatibles(codeFonction);
        if (codesFonctions.Count == 0)
        {
            codesFonctions = new List<int> { codeFonction };
        }

        string placeholders = string.Join(", ", codesFonctions.Select((_, index) => $"@CodeFonction{index}"));
        string query = $@"
            SELECT COUNT(*) FROM MEDECIN m
            WHERE m.CODE_FONCTION IN ({placeholders})
              AND LOWER(TRIM(m.STATUT)) = 'actif'
              AND NOT EXISTS (
                  SELECT 1 FROM RENDEZ_VOUS r
                  WHERE r.ID_HER_2 = m.ID_MEDECIN
                    AND r.DATEHEURERDV = @DateHeure
                    AND r.STATUT = 'PLANIFIE'
              );";

        using var cmd = new NpgsqlCommand(query, conn);
        for (int i = 0; i < codesFonctions.Count; i++)
        {
            cmd.Parameters.AddWithValue($"CodeFonction{i}", codesFonctions[i]);
        }
        cmd.Parameters.AddWithValue("DateHeure", dateHeureBloc);

        var count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    private List<int> ObtenirCodesFonctionsCompatibles(int codeFonction)
    {
        if (codeFonction <= 0)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT code_fonction FROM fonction ORDER BY code_fonction", conn);
            using var reader = cmd.ExecuteReader();

            var tous = new List<int>();
            while (reader.Read())
            {
                tous.Add(reader.GetInt32(0));
            }

            return tous;
        }

        return new ServiceMedicalLookupService().ObtenirCodesFonctionsCompatibles(codeFonction);
    }
}
