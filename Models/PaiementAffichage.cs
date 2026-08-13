namespace Patients.Models;

// Une ligne de facture prete a afficher (paiement + nom du patient +
// nombre de relances deja envoyees pour ce paiement).
public class PaiementAffichage
{
    public string NumeroPaiement { get; set; } = string.Empty;
    public string NumeroConsultation { get; set; } = string.Empty;
    public string NumeroRdv { get; set; } = string.Empty;
    public string PatientNom { get; set; } = string.Empty;

    // ACOMPTE (paiement en avance, avant consultation) ou NORMAL (solde,
    // apres consultation).
    public string TypePaiement { get; set; } = "NORMAL";

    // Tarif total du rendez-vous (taux horaire du medecin), necessaire
    // pour distinguer un acompte "Complète" (couvre tout) d'un acompte
    // "Avance" (partiel) dans la colonne Type de l'historique.
    public decimal MontantTotalRdv { get; set; }

    public string TypeAffiche => TypePaiement switch
    {
        "NORMAL" => "Reste",
        "ACOMPTE" when Montant >= MontantTotalRdv && MontantTotalRdv > 0 => "Complète",
        "ACOMPTE" => "Avance",
        _ => TypePaiement
    };

    // Date de creation de la facture (= DATEPAIEMENT tant que le
    // paiement n'est pas encore regle ; devient la date de reglement
    // une fois confirme).
    public DateTime DateFacture { get; set; }

    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = string.Empty;
    public bool EstPaye { get; set; }
    public int NombreRelances { get; set; }

    // Vrai une fois qu'une facture a ete affichee/confirmee pour ce
    // paiement (page Paiements, colonne "Facturation").
    public bool EstFacture { get; set; }
    public string FacturationAffichee => EstFacture ? "✔️" : "❌";

    // Nombre de jours ecoules depuis la creation de la facture, utilise
    // par la regle d'annulation automatique en cas d'impaye.
    public int JoursEcoules => (DateTime.Now - DateFacture).Days;

    public string MontantAffiche => Montant.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " Ar";
}
