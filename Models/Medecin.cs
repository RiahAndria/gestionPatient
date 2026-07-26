namespace Patients.Models;

// public class Medecin : Personne
// {
//     public string statut {get; set; } = string.Empty; 
//     public string numero_ordre {get; set; } = string.Empty;
//     public string nom_fonction {get; set; } = string.Empty;
//     public int code_fonction {get; set; } = 0;
//     public int taux_horaire {get; set; } 
// }


public class Medecin : Personne
{
    public string statut {get; set; } = string.Empty; 
    public string numero_ordre {get; set; } = string.Empty;
    public string nom_fonction {get; set; } = string.Empty;
    public int code_fonction {get; set; } = 0;
    public decimal taux_horaire {get; set; } 
}
