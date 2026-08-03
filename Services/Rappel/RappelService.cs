using System.IO;
using Microsoft.Extensions.Configuration;

namespace Patients.Services;

// Service Rappel, fractionne en 2 fichiers :
//   - RappelService.Generation.cs : creation des rappels (24h)
//   - RappelService.Lecture.cs    : consultation/gestion des notifications
public partial class RappelService
{
    private readonly string _connectionString;

    public RappelService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
}
