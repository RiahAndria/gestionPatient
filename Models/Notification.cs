namespace Patients.Models;

public class Notification
{
    public string NumeroNotif { get; set; } = string.Empty; // Clé primaire
    public string NumeroRdv { get; set; } = string.Empty; // FK
    public string TexteNotif { get; set; } = string.Empty;
}