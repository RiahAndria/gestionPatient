using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Patients.Models;
using Patients.Services;
using Patients.Views.Patient;

namespace Patients.Views.Patient.Components;

public partial class TableauPatient : UserControl
{
    private readonly PatientService _patientService = new PatientService();

    public TableauPatient()
    {
        InitializeComponent();
        ChargerDonnees();
    }

    public void ChargerDonnees()
    {
        try
        {
            // Charger la liste depuis la BDD dans la variable globale
            MainWindow.ListePatientsGlobal = _patientService.ObtenirTousLesPatients();
            RafraichirTableau();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des données depuis PostgreSQL : {ex.Message}", 
                            "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void RafraichirTableau()
    {
        dgPatients.ItemsSource = null;
        dgPatients.ItemsSource = MainWindow.ListePatientsGlobal;
    }

    private void btnRecherche_Click(object sender, RoutedEventArgs e)
    {
        string filtre = txtRecherche.Text.ToLower().Trim();
        
        if (string.IsNullOrWhiteSpace(filtre))
        {
            dgPatients.ItemsSource = MainWindow.ListePatientsGlobal;
        }
        else
        {
            var resultat = MainWindow.ListePatientsGlobal.FindAll(p => 
                p.NumeroDossier.ToLower().Contains(filtre) || 
                p.Nom.ToLower().Contains(filtre) || 
                p.Prenom.ToLower().Contains(filtre));
            dgPatients.ItemsSource = resultat;
        }
    }

    private void dgPatients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgPatients.SelectedItem is Models.Patient patientSelectionne)
        {
            DetailPatientWindow detailWindow = new DetailPatientWindow(patientSelectionne);
            detailWindow.ShowDialog();
        }
    }
}