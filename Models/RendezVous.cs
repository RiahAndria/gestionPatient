namespace Patients.Models;

public class RendezVous
{
   public string NumRendezVous { get; set; } = string.Empty;
   public string PatientID { get; set; } = string.Empty;
   public string MedecinID { get; set; } = string.Empty;
   public DateTime DateHeure { get; set; }
   public string Motif { get; set; } = string.Empty;
   public string Statut { get; set; } = "PLANIFIE"; // PLANIFIE | ANNULE | TERMINE
   public string? MotifAnnulation { get; set; }
}