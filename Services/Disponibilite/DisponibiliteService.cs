using System.IO;
using Microsoft.Extensions.Configuration;

namespace Patients.Services;

// Service Disponibilite, fractionne en 2 fichiers :
//   - DisponibiliteService.Creation.cs : CreerDisponibilite
//   - DisponibiliteService.Lecture.cs  : obtenirLesTempsMedecin
public partial class DisponibiliteService
{
    private readonly string _connectionString;

    public DisponibiliteService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
}
