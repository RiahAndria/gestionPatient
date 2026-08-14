namespace Patients.Models;

public class Disponibilite
{
    public string id_medecin {get; set; } = string.Empty ;
    public DateTime date_disponibilite {get; set; }
    public int numero_bloc {get; set; } = 0;
}