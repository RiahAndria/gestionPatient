namespace Patients.Models;

public class RendezVous
{
   public string Id { get; set; } = string.Empty;
   public string Patient { get; set; } = string.Empty;
   public string Medecin { get; set; } = string.Empty;
   public DateTime Date { get; set; }
   public string Heure { get; set; } = string.Empty;
   public string Motif { get; set; } = string.Empty;
}