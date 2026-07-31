using System.IO;

namespace Patients.Models;

public class Temps:Disponibilite
{
    public DateTime heure_debut ;
    public DateTime heure_fin;
    public Boolean est_disponible = true;
    public Boolean est_reserve = false;
}