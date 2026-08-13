namespace Patients.Models;

// Un "service medical" tel que presente a l'etape 2 de l'assistant de
// prise de rendez-vous (ex: Cardiologie, Medecine Generale...).
//
// Il n'existe pas de table SERVICE dediee en base : un service
// correspond a une FONCTION de medecin (voir ServiceMedicalLookupService
// pour la derivation). Ce modele reste volontairement independant de
// Fonction pour ne pas coupler l'UI au schema de la table FONCTION -
// si un jour une vraie table SERVICE est ajoutee, seul
// ServiceMedicalLookupService aura besoin de changer.
public class ServiceMedical
{
    // Code de la fonction associee (FONCTION.CODE_FONCTION), utilise
    // pour retrouver les medecins de ce service a l'etape 4.
    public int CodeFonction { get; set; }

    public string NomService { get; set; } = string.Empty;

    // Regle metier de l'etape 3 : si vrai, l'option "Aujourd'hui" est
    // desactivee et le calendrier ne propose des dates qu'a partir de
    // J+2.
    public bool NecessiteDelai { get; set; }

    public string Indication => NecessiteDelai
        ? "Réservation à l'avance requise"
        : "Disponible aujourd'hui";
}
