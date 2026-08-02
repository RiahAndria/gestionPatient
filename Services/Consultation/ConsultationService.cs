using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    // Resultat detaille de l'enregistrement d'une consultation : indique
    // si la facture a bien ete generee (et pour quel montant), pour que
    // l'ecran puisse informer clairement l'utilisateur.
    public class ResultatEnregistrementConsultation
    {
        public bool Succes { get; set; }
        public string? MessageErreur { get; set; }
        public bool FactureCreee { get; set; }
        public decimal MontantFacture { get; set; }
        public string? MessageFacture { get; set; }
    }

    public partial class ConsultationService
    {
        private readonly string _connectionString;
        private readonly PaiementService _paiementService = new();

        public ConsultationService()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
    }
}
