namespace Patients.Models
{
    public class Consultation
    {
        // Cle primaire : NUMEROCONSULTATION (VARCHAR)
        public string NumeroConsultation { get; set; } = string.Empty;

        // Cle etrangere obligatoire vers RENDEZ_VOUS (contrainte NOT NULL
        // UNIQUE ajoutee lors de la migration de schema : une consultation
        // decoule toujours d'un rendez-vous precis, et un rendez-vous ne
        // peut generer qu'une seule consultation).
        public string NumeroRdv { get; set; } = string.Empty;

        public string Diagnostique { get; set; } = string.Empty;
        public string NotesMedicales { get; set; } = string.Empty;
        public Ordonnance? OrdonnanceAssociee { get; set; }
    }
}
