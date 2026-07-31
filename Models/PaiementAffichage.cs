namespace Patients.Models;

public class PaiementAffichage
{
    public string NumeroPaiement { get; set; } = string.Empty;
    public string NumeroConsultation { get; set; } = string.Empty;
    public string NumeroRdv { get; set; } = string.Empty;
    public string PatientNom { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; } // = DATEPAIEMENT tant que STATUT=false (date de creation de la facture)
    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = string.Empty;
    public bool EstPaye { get; set; }
    public int NombreRelances { get; set; }

    public int JoursEcoules => (DateTime.Now - DateFacture).Days;

    public string MontantAffiche => Montant.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " Ar";
}