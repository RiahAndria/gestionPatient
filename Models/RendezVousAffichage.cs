namespace Patients.Models;

// Options utilisees pour peupler les ComboBox patient/medecin du
// formulaire de creation de rendez-vous (evite de charger l'objet
// Patient/Medecin complet, on n'a besoin que du strict necessaire pour
// l'affichage et la selection).
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

// Une ligne de la grille des rendez-vous : le rendez-vous, avec les noms
// du patient et du medecin deja lisibles (au lieu de leurs ID bruts).
public class RendezVousAffichage
{
    public string NumeroRdv { get; set; } = string.Empty;
    public string PatientNom { get; set; } = string.Empty;
    public string MedecinNom { get; set; } = string.Empty;
    public DateTime DateHeure { get; set; }
    public string Motif { get; set; } = string.Empty;

    // Valeur telle que stockee en base : PLANIFIE, ANNULE ou TERMINE.
    public string Statut { get; set; } = string.Empty;

    // Nombre d'alertes rendez-vous deja envoyees (notifications
    // TYPE_NOTIF = 'RESERVATION' liees a ce RDV), affiche dans la
    // colonne Alertes de la grille.
    public int NombreAlertes { get; set; }

    // Jours restants avant la date du rendez-vous (peut etre negatif
    // si la date est passee).
    public int JoursRestants => (DateHeure.Date - DateTime.Today).Days;

    // Alerte cliquable uniquement pour un RDV encore a venir et non
    // annule/termine ; sinon rien a afficher.
    public bool AlerteActivable => Statut == "PLANIFIE" && JoursRestants >= 0;

    public string AlerteAffichee => AlerteActivable ? $"{NombreAlertes}🔔 J-{JoursRestants}" : "—";

    // Version en francais du statut, pour l'affichage a l'ecran.
    public string StatutAffiche => Statut switch
    {
        "PLANIFIE" => "Planifié",
        "ANNULE" => "Annulé",
        "TERMINE" => "Terminé",
        _ => Statut
    };

    // Couleur du badge affiche dans la grille, selon le statut.
    public System.Windows.Media.Brush CouleurStatut => Statut switch
    {
        "PLANIFIE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x25, 0x63, 0xEB)),
        "ANNULE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xDC, 0x26, 0x26)),
        "TERMINE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0xA3, 0x4A)),
        _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x72, 0x80)),
    };
}
