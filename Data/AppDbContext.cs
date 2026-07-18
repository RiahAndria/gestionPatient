using Microsoft.EntityFrameworkCore;
using Patients.Models;

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
            // La chaîne de connexion PostgreSQL (à adapter avec ton mot de passe)
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=gestion_patients_db;Username=postgres;Password=riah1234");
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