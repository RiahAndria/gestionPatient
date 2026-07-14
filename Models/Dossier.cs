namespace Patients.Models;

public class Dossier
{
   public string NumeroDossier { get; set; } = string.Empty;
   public string Patient { get; set; } = string.Empty;
   public float Poids { get; set; }
   public float Taille { get; set; }
   public string GroupeSanguin { get; set; } = string.Empty; // Pas obligatoire
   public string Allergies { get; set; } = string.Empty;
   public string Diagnotique { get; set; } = string.Empty;
}