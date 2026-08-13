namespace Patients.Models;

// Toutes les informations necessaires pour afficher une facture (page
// Paiements, bouton "Facturer"). Purement lecture seule - pas
// d'impression ni d'export, juste un affichage a l'ecran.
public class FactureDetail
{
    public string NumeroPaiement { get; set; } = string.Empty;
    public string NumeroRdv { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }

    public string PatientNom { get; set; } = string.Empty;
    public string PatientMatricule { get; set; } = string.Empty;

    public string MedecinNom { get; set; } = string.Empty;
    public string MedecinFonction { get; set; } = string.Empty;

    public string TypePaiement { get; set; } = string.Empty; // ACOMPTE ou NORMAL
    public string TypePaiementAffiche => TypePaiement == "ACOMPTE" ? "Acompte" : "Solde";

    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = string.Empty;
}
