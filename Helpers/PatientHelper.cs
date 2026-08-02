using System;
using System.Text.RegularExpressions;

namespace Patients.Helpers;

public static class PatientHelper
{
    private static readonly Regex NomRegex = new(@"^[\p{L}\s'-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex TelephoneRegex = new(@"^\d{10}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool EstNomValide(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return false;
        }

        return NomRegex.IsMatch(valeur.Trim());
    }

    public static bool EstDateNaissanceValide(DateTime? dateNaissance)
    {
        if (dateNaissance is null)
        {
            return false;
        }

        var date = dateNaissance.Value.Date;
        var aujourdHui = DateTime.Today;
        var dateMinimum = aujourdHui.AddYears(-100);

        return date <= aujourdHui && date >= dateMinimum;
    }

    public static bool EstEmailValide(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return true;
        }

        return EmailRegex.IsMatch(valeur.Trim());
    }

    public static bool EstTelephoneValide(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return true;
        }

        return TelephoneRegex.IsMatch(valeur.Trim());
    }

    public static string ObtenirTitrePatient(string? genre, DateTime dateNaissance)
    {
        var age = CalculerAge(dateNaissance);

        if (age <= 17)
        {
            return "Enfant";
        }

        return string.Equals(genre, "Femme", StringComparison.OrdinalIgnoreCase) ? "Mme" : "Mr";
    }

    public static string ObtenirDetailPatient(string? genre, DateTime dateNaissance)
    {
        var age = CalculerAge(dateNaissance);
        var genreAffiche = string.IsNullOrWhiteSpace(genre) ? "Non précisé" : genre;
        return $"Genre : {genreAffiche} • Né(e) le {dateNaissance:dd/MM/yyyy} • {age} ans";
    }

    public static int CalculerAge(DateTime dateNaissance)
    {
        if (dateNaissance == default)
        {
            return 0;
        }

        var aujourdHui = DateTime.Today;
        var age = aujourdHui.Year - dateNaissance.Year;

        if (dateNaissance.Date > aujourdHui.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
