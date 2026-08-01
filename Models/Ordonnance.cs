namespace Patients.Models
{
    public class Ordonnance
    {
        // Clé primaire : NUMEROPRESCRIPTION (VARCHAR)
        public string NumeroPrescription { get; set; } = string.Empty;

        // Clé étrangère vers CONSULTATION
        public string NumeroConsultation { get; set; } = string.Empty;

        // Informations sur le traitement
        public string Traitement { get; set; } = string.Empty;
        public string Duree { get; set; } = string.Empty; 
        public string Diagnostique { get; set; } = string.Empty;
    }
}