

public class Personne
{
    public string Id { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string AgeAffiche
    {
        get
        {
            if (DateNaissance == default)
            {
                return "-";
            }

            var aujourdHui = DateTime.Today;
            var age = aujourdHui.Year - DateNaissance.Year;

            if (DateNaissance.Date > aujourdHui.AddYears(-age))
            {
                age--;
            }

            return age >= 0 ? $"{age} ans" : "-";
        }
    }

    // Nom + prenom affichables ensemble (ex: liste des patients de
    // l'etape 1 de l'assistant de rendez-vous), sur le meme principe
    // que AgeAffiche ci-dessus.
    public string NomComplet => $"{Nom} {Prenom}".Trim();
}
