namespace Patients.Models;

public class Consultation
{
    public string NumeroConsultation { get; set; } = string.Empty; // Clé primaire
    public string Diagnostique { get; set; } = string.Empty;
    public string NotesMedicales { get; set; } = string.Empty;
}