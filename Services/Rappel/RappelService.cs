using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Patients.Services;

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
