namespace Patients.Models;

public class Ordonance
{
    public string NumeroPrescritption { get; set; } = string.Empty; // Clé primaire
    public string NumeroConsultation { get; set; } = string.Empty; // FK vers Consultation
    public string Traitement { get; set; } = string.Empty;
    public string Duree { get; set; } = string.Empty;
    public string Diagnostique { get; set; } = string.Empty;
}