using System.IO;
using Microsoft.Extensions.Configuration;

namespace Patients.Services;

// Resultat d'une tentative d'acompte : au-dela du succes/echec, precise
// si le rendez-vous est desormais entierement regle, ou combien il
// reste a payer.
public class ResultatAcompte
{
    public bool Succes { get; set; }
    public string? MessageErreur { get; set; }
    public bool PaiementComplet { get; set; }
    public decimal MontantRestant { get; set; }
}

// Resultat de la generation de facture apres consultation : precise si
// une facture a reellement ete creee (elle peut ne pas l'etre si le
// rendez-vous etait deja entierement paye par acompte).
public class ResultatFacture
{
    public bool FactureCreee { get; set; }
    public decimal Montant { get; set; }
    public string? Message { get; set; }
}

// Service Paiement, fractionne en plusieurs fichiers par thematique
// (fichier .cs = base commune, les autres portent le nom
// PaiementService.<Thematique>.cs) :
//   - PaiementService.Acompte.cs  : paiement en avance
//   - PaiementService.Facture.cs  : solde apres consultation
//   - PaiementService.Lecture.cs  : listes affichees a l'ecran
//   - PaiementService.Relance.cs  : confirmation, relances, impayes
public partial class PaiementService
{
    private readonly string _connectionString;

    // Seuils de la politique de relance/annulation, modifiables ici en
    // un seul endroit.
    private const int NB_RELANCES_MAX = 3;
    private const int DELAI_ANNULATION_JOURS = 7;

    public PaiementService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
}
