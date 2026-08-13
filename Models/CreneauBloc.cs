namespace Patients.Models;

// Un creneau horaire "large" propose a l'etape 4 de l'assistant
// (Matin / Apres-midi / Fin de journee), qui regroupe plusieurs blocs
// de 15 minutes de la table TEMPS (voir DisponibiliteService).
//
// Le decoupage en NUMERO_BLOC vient de DisponibiliteService.Creation :
// bloc 3 = 08h-12h, bloc 4 = 12h-16h, bloc 5 = 16h-20h. Seuls ces 3
// blocs sont proposes ici, pour rester dans les horaires d'ouverture
// (08h00-18h00) deja utilises ailleurs dans l'application ; le bloc 5
// est donc affiche/borne jusqu'a 18h00 seulement.
public class CreneauBloc
{
    public int NumeroBloc { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
}

// Un medecin disponible pour un service/date/creneau donnes, avec les
// informations a afficher a l'etape 4 (nom, fonction, taux horaire).
public class MedecinDisponible
{
    public string Id { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Fonction { get; set; } = string.Empty;
    public decimal TauxHoraire { get; set; }
}
