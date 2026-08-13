using Patients.Models;

namespace Patients.Views.RendezVous.Assistant;

// Etat partage entre les 7 etapes de l'assistant de creation de
// rendez-vous (voir NouveauRendezVousWindow). Une seule instance vit
// pendant toute la duree de vie de la fenetre et est transmise a
// chaque etape : chacune y lit ce dont elle a besoin et y ecrit son
// resultat avant de passer a la suivante.
public class AssistantRendezVousState
{
    // Pre-selection optionnelle, quand l'assistant est ouvert depuis la
    // fiche d'un patient (ex: bouton "Nouveau rendez-vous" sur
    // DetailPatientWindow) plutot que depuis la liste generale.
    public string? PatientIdPreselectionne { get; set; }

    // ---- Etape 1 : patient (lecture seule, vient d'un autre module) ----
    public Patients.Models.Patient? Patient { get; set; }

    // ---- Etape 2 : service medical ----
    public ServiceMedical? Service { get; set; }

    // ---- Etape 3 : type de rendez-vous ----
    public bool RdvAujourdHui { get; set; }
    public DateTime? DateChoisie { get; set; }

    // ---- Etape 4 : creneau + medecin ----
    public CreneauBloc? Creneau { get; set; }
    public MedecinDisponible? Medecin { get; set; }

    // ---- Etape 5 : recapitulatif ----
    public string Motif { get; set; } = string.Empty;

    // Renseigne au moment de la creation effective du rendez-vous
    // (fin de l'etape 5), pour que les etapes 6 et 7 sachent sur quel
    // rendez-vous agir.
    public string? NumeroRdvCree { get; set; }
    public decimal MontantTotal { get; set; }

    // ---- Etape 6 : paiement ----
    public bool PaiementEffectue { get; set; }
    public bool PaiementComplet { get; set; }
    public decimal MontantVerse { get; set; }
    public decimal MontantRestant { get; set; }
    public string ModePaiementChoisi { get; set; } = string.Empty;

    // Date et heure effectives du rendez-vous, calculees a partir de la
    // date choisie (etape 3) et du debut du creneau retenu (etape 4).
    public DateTime DateHeureRdv => (DateChoisie ?? DateTime.Today).Date + (Creneau?.HeureDebut ?? TimeSpan.Zero);
}
