using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services;

public partial class PatientService
{
    private readonly string _connectionString;

    public PatientService()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private Patients.Models.Patient MapToPatient(NpgsqlDataReader reader)
    {
        return new Patients.Models.Patient
        {
            Id = reader.GetString(0),
            Nom = reader.GetString(1),
            Prenom = reader.GetString(2),
            DateNaissance = reader.GetDateTime(3),
            Genre = reader.GetString(4),
            Adresse = reader.GetString(5),
            Telephone = reader.GetString(6),
            Email = reader.GetString(7),
            NumeroDossier = reader.GetString(8),
            NumeroAssurance = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
        };
    }
}