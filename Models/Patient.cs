namespace Patients.Models;

public class Patient : Personne
{
    public string NumeroDossier { get; set; } = string.Empty;
    public string NumeroAssurance { get; set; } = string.Empty; 
    public string MedecinTraitant { get; set; } = string.Empty; 
}
