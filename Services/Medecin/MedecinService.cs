using System.IO;
using Microsoft.Extensions.Configuration;

namespace Medecins.Services;

// Service Medecin, fractionne en plusieurs fichiers (namespace et nom
// de classe conserves a l'identique pour ne rien casser cote appelants) :
//   - MedecinService.Creation.cs     : AjouterMedecin
//   - MedecinService.Lecture.cs      : listes, recherches, verifications
//   - MedecinService.Modification.cs : ModificationMedecin
//   - MedecinService.Suppression.cs  : SupprimerMedecin
public partial class MedecinService
{
    public string message { get; set; } = string.Empty;
    private readonly string _connectionString;

    public MedecinService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

       _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
}
