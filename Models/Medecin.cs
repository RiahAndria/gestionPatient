namespace Patients.Models;

public class Medecin : Personne
{
    public string  Statut { get; set; } = string.Empty; // Statut du médecin (ex: interne, résident, spécialiste, etc.)
    public string Fonction { get; set; } = string.Empty; // Fonction du médecin (ex: chirurgien, généraliste, pédiatre, etc.)
    public int TauxHoraire { get; set; }
    public bool EstDisponible { get; set; } = true; // Indique si le médecin est disponible pour des consultations
}
