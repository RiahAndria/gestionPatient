namespace Patients.Models;

public class Dossier
{
   public string NumeroDossier { get; set; } = string.Empty;
   public string PatientID { get; set; } = string.Empty;
   public decimal Poids { get; set; }
   public decimal Taille { get; set; }
   public string GroupeSanguin { get; set; } = string.Empty; // Pas obligatoire
   public string Allergies { get; set; } = string.Empty;
   public string Traitement { get; set; } = string.Empty;
   public string Antecedents { get; set; } = string.Empty;
}