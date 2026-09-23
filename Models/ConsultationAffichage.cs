namespace Patients.Models
{
    public class ConsultationAffichage
    {
        public string NumeroConsultation { get; set; } = string.Empty;
        public string NumeroRdv { get; set; } = string.Empty;
        public string NumeroDossier { get; set; } = string.Empty;
        public DateTime? DateConsultation { get; set; }
        public string Diagnostique { get; set; } = string.Empty;
        public string NotesMedicales { get; set; } = string.Empty;
        public decimal? Poids { get; set; }
        public decimal? Taille { get; set; }
        public string GroupeSanguin { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string Traitement { get; set; } = string.Empty;
        public string Antecedents { get; set; } = string.Empty;
        public Ordonnance? OrdonnanceAssociee { get; set; }

        // Informations du médecin
        public string NomMedecin { get; set; } = string.Empty;
        public string FonctionMedecin { get; set; } = string.Empty;
    }
}
