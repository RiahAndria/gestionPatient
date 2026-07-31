namespace Patients.Models;

// Utilisés pour peupler les ComboBox patient/médecin sans charger tout l'objet Patient/Medecin complet (plus léger, et découplé du reste).
public class PatientOption
{
    public string Id { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Matricule { get; set; } = string.Empty;
}

public class MedecinOption
{
    public string IdHer2 { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Fonction { get; set; } = string.Empty;
}

// Ligne "prête à afficher" pour la grille des rendez-vous : le RDV brut + les noms lisibles du patient et du médecin (au lieu de deux ID bruts).
public class RendezVousAffichage
{
    public string NumeroRdv { get; set; } = string.Empty;
    public string PatientNom { get; set; } = string.Empty;
    public string MedecinNom { get; set; } = string.Empty;
    public DateTime DateHeure { get; set; }
    public string Motif { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty; // PLANIFIE | ANNULE | TERMINE (valeurs telles que stockées en base)

    public string StatutAffiche => Statut switch
    {
        "PLANIFIE" => "Planifié",
        "ANNULE" => "Annulé",
        "TERMINE" => "Terminé",
        _ => Statut
    };

    public System.Windows.Media.Brush CouleurStatut => Statut switch
    {
        "PLANIFIE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x25, 0x63, 0xEB)),
        "ANNULE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xDC, 0x26, 0x26)),
        "TERMINE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0xA3, 0x4A)),
        _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x72, 0x80)),
    };
}