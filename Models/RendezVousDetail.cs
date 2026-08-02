namespace Patients.Models;

// Toutes les informations affichees dans la fenetre de detail d'un
// rendez-vous (ouverte par double-clic depuis la liste), sur le meme
// principe que DetailPatientWindow et DetailMedecinWindow.
public class RendezVousDetail
{
    public string NumeroRdv { get; set; } = string.Empty;
    public DateTime DateHeure { get; set; }
    public string Motif { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public string? MotifAnnulation { get; set; }

    public string PatientId { get; set; } = string.Empty;
    public string PatientNom { get; set; } = string.Empty;
    public string PatientTelephone { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientMatricule { get; set; } = string.Empty;

    public string MedecinNom { get; set; } = string.Empty;
    public string MedecinFonction { get; set; } = string.Empty;
    public decimal MedecinTauxHoraire { get; set; }

    public string StatutAffiche => Statut switch
    {
        "PLANIFIE" => "Planifié",
        "ANNULE" => "Annulé",
        "TERMINE" => "Terminé",
        _ => Statut
    };
}
