namespace Patients.Models;

public class Patient : Personne
{
    public string NumeroDossier { get; set; } = string.Empty;
    public string NumeroAssurance { get; set; } = string.Empty; // Pas forcément utile pour le moment vu que pas gérable
    public string NumeroPrescritption { get; set; } = string.Empty;
    public string MedecinTraitant { get; set; } = string.Empty; 
}
