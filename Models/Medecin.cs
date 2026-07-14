namespace Patients.Models;

public class Medecin : Personne
{
    public string  Statut { get; set; } = string.Empty;
    public string fonction { get; set; } = string.Empty;
    public int TauxHoraire { get; set; }

}
