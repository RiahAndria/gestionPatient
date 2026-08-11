namespace Patients.Models
{
    public class AgendaJournee
    {
        public DateTime Date { get; set; }
        public string Id_medecin { get; set; } = string.Empty;
        public List<Temps> Creneaux15Min { get; set; } = new List<Temps>();
    }
}