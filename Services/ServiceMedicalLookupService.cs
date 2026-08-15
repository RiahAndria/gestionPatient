using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Patients.Models;

namespace Patients.Services;

// Derive la liste des "services medicaux" (etape 2 de l'assistant de
// rendez-vous) a partir des fonctions de medecins deja enregistrees en
// base (table FONCTION, via FonctionService) : un service = une
// fonction pour laquelle au moins un medecin existe.
//
// La regle "necessite un delai / disponible aujourd'hui" n'a pas
// d'equivalent en base pour l'instant (voir Models/ServiceMedical.cs) :
// on part sur une regle codee en dur ci-dessous, ajustable simplement
// dans _fonctionsSansDelai. A terme, si le besoin se confirme, cette
// information devrait migrer vers une vraie colonne (ex:
// FONCTION.NECESSITE_DELAI) plutot que de rester en dur ici.
public class ServiceMedicalLookupService
{
    private readonly FonctionService _fonctions = new();

    // Fonctions pour lesquelles un rendez-vous "aujourd'hui" reste
    // possible (consultations courantes, sans plateau technique
    // particulier). Toute fonction absente de cette liste est
    // consideree comme necessitant une reservation a l'avance.
    private static readonly HashSet<string> _fonctionsSansDelai = new(StringComparer.OrdinalIgnoreCase)
    {
        "Médecine Générale",
        "Medecine Generale",
        "Généraliste",
        "Generaliste",
    };

    public List<ServiceMedical> ObtenirServicesDisponibles()
    {
        var fonctions = _fonctions.recupererLeListeDesFonctions();

        var services = fonctions
            .GroupBy(f => NormaliserNomFonction(f.nom_fonction))
            .Select(g => g.OrderBy(f => f.code_fonction).First())
            .OrderBy(f => f.nom_fonction, StringComparer.OrdinalIgnoreCase)
            .Select(f => new ServiceMedical
            {
                CodeFonction = f.code_fonction,
                NomService = f.nom_fonction,
                NecessiteDelai = !EstFonctionSansDelai(f.nom_fonction)
            })
            .ToList();

        return services;
    }

    public List<int> ObtenirCodesFonctionsCompatibles(int codeFonction)
    {
        var fonctions = _fonctions.recupererLeListeDesFonctions();
        var service = fonctions.FirstOrDefault(f => f.code_fonction == codeFonction);

        if (service is null)
        {
            return new List<int> { codeFonction };
        }

        string nomNormalise = NormaliserNomFonction(service.nom_fonction);

        return fonctions
            .Where(f => NormaliserNomFonction(f.nom_fonction) == nomNormalise)
            .Select(f => f.code_fonction)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    private static bool EstFonctionSansDelai(string nomFonction)
    {
        if (string.IsNullOrWhiteSpace(nomFonction))
        {
            return false;
        }

        return _fonctionsSansDelai.Contains(nomFonction)
            || _fonctionsSansDelai.Contains(NormaliserNomFonction(nomFonction));
    }

    private static string NormaliserNomFonction(string nomFonction)
    {
        if (string.IsNullOrWhiteSpace(nomFonction))
        {
            return string.Empty;
        }

        string sansAccents = new string(
            nomFonction.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray());

        return sansAccents
            .Trim()
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace("  ", " ")
            .ToLowerInvariant();
    }
}
