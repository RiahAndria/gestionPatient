namespace Patients.Models
{
    public class Consultation
    {
        // Clé primaire : NUMEROCONSULTATION (VARCHAR)
        public string NumeroConsultation { get; set; } = string.Empty;
        public string Diagnostique { get; set; } = string.Empty;
        public string NotesMedicales { get; set; } = string.Empty;
        public Ordonnance? OrdonnanceAssociee { get; set; }
    }

}