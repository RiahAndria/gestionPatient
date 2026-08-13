namespace Patients.Models;

// Une ligne de la section "Paiements non complets" de la page
// Paiements : regroupe 2 origines differentes derriere une seule
// liste, distinguees par EstAcompteEnAttente :
//   - un acompte verse mais qui ne couvre pas encore tout le tarif
//     (rendez-vous pas encore realise) : NumeroPaiement est vide, on
//     "Regle" directement via le numero du rendez-vous ;
//   - une facture NORMALE (solde post-consultation) pas encore payee :
//     NumeroPaiement est renseigne, on "Regle" via ConfirmerPaiement.
public class PaiementIncomplet
{
    public string? NumeroPaiement { get; set; }
    public string NumeroRdv { get; set; } = string.Empty;
    public string NumeroAffiche => string.IsNullOrEmpty(NumeroPaiement) ? NumeroRdv : NumeroPaiement;

    public bool EstAcompteEnAttente => string.IsNullOrEmpty(NumeroPaiement);

    public string PatientNom { get; set; } = string.Empty;

    public decimal MontantRestant { get; set; }
    public string MontantRestantAffiche => MontantRestant.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " Ar";

    // Date limite de reglement = date du rendez-vous lui-meme (regle
    // metier demandee : le solde doit etre regle au plus tard le jour
    // de la consultation).
    public DateTime DateLimite { get; set; }

    public int NombreAlertes { get; set; }
    public int JoursRestants => (DateLimite.Date - DateTime.Today).Days;
    public string AlerteAffichee => JoursRestants >= 0 ? $"{NombreAlertes}🔔 J-{JoursRestants}" : $"{NombreAlertes}🔔 En retard";
}
