using Microsoft.EntityFrameworkCore;
using Patients.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Patients.Data
{
    public class AppDbContext : DbContext
    {
        // Liste des tables dans la base de donnée
        public DbSet<Personne> Personnes { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Medecin> Medecins { get; set; }
        public DbSet<Dossier> Dossiers { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Ordonance> Ordonances { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 1. On va chercher le fichier appsettings.json
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // 2. On récupère la chaîne de connexion "DefaultConnection"
                string connectionString = configuration.GetConnectionString("DefaultConnection")!;
                
                // 3. On l'injecte dans Npgsql
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Héritage de personne -> Patient et Personne -> Medecin
            modelBuilder.Entity<Personne>().ToTable("PERSONNE");
            modelBuilder.Entity<Patient>().ToTable("PATIENT");
            modelBuilder.Entity<Medecin>().ToTable("MEDECIN");

            // Mappage des autres tables (pour respecter les noms en MAJUSCULES de la BDD)
            modelBuilder.Entity<Dossier>().ToTable("DOSSIER_MEDICAL").HasKey(d => d.NumeroDossier);
            modelBuilder.Entity<RendezVous>().ToTable("RENDEZ_VOUS").HasKey(r => r.NumRendezVous);
            modelBuilder.Entity<Consultation>().ToTable("CONSULTATION").HasKey(c => c.NumeroConsultation);
            modelBuilder.Entity<Ordonance>().ToTable("ORDONANCE").HasKey(o => o.NumeroPrescritption);
            modelBuilder.Entity<Paiement>().ToTable("PAIEMENT").HasKey(p => p.NumeroPaiement);
            modelBuilder.Entity<Notification>().ToTable("NOTIFICATION").HasKey(n => n.NumeroNotif);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}