namespace Patients.Models;

public class Notification
{
    // Clé primaire : NUMERONOTIF (VARCHAR)
    public string NumeroNotif { get; set; } = string.Empty;

    // Clé étrangère vers RENDEZ_VOUS
    public string NumeroRdv { get; set; } = string.Empty;

    // Le texte du rappel
    public string TexteNotif { get; set; } = string.Empty;

    public DateTime DateNotif { get; set; }
    public bool Lu { get; set; }
}
