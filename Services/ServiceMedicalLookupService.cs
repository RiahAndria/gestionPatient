using System.Collections.Generic;
using System.Linq;
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
    private static readonly HashSet<string> _fonctionsSansDelai = new(System.StringComparer.OrdinalIgnoreCase)
    {
        "Médecine Générale",
        "Medecine Generale",
        "Généraliste",
        "Generaliste",
    };

    public List<ServiceMedical> ObtenirServicesDisponibles()
    {
        return _fonctions.recupererLeListeDesFonctions()
            .Select(f => new ServiceMedical
            {
                CodeFonction = f.code_fonction,
                NomService = f.nom_fonction,
                NecessiteDelai = !_fonctionsSansDelai.Contains(f.nom_fonction)
            })
            .OrderBy(s => s.NomService)
            .ToList();
    }
}
