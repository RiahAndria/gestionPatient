using System.Windows;
using Patients.Models;

namespace Patients;

public partial class DetailPatientWindow : Window
{
    public DetailPatientWindow(Patient patient)
    {
        InitializeComponent();
        
        // Remplissage des données complètes (pas complet pour le moment XD)
        lblMatricule.Text = patient.NumeroDossier;
        lblIdentite.Text = $"{patient.Nom} {patient.Prenom} ({patient.Genre} - Né(e) le {patient.DateNaissance:dd/MM/yyyy})";
        lblContact.Text = $"Tél: {patient.Telephone} | Email: {patient.Email}\nAdresse: {patient.Adresse}";
        lblAssurance.Text = string.IsNullOrWhiteSpace(patient.NumeroAssurance) ? "Aucune assurance enregistrée" : patient.NumeroAssurance;
        lblMedecin.Text = string.IsNullOrWhiteSpace(patient.MedecinTraitant) ? "Non spécifié" : patient.MedecinTraitant;
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}