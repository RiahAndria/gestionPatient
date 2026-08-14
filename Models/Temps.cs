using System.IO;

namespace Patients.Models;

public class Temps:Disponibilite
{
    public int id_temps {get; set; }
    public DateTime heure_debut {get; set; }
    public DateTime heure_fin {get; set; }
    public Boolean est_disponible {get; set; } = true;
    public Boolean est_reserve {get; set; } = false;
}

