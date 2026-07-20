using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients;

public partial class MainWindow : Window
{
    public static List<Patient> ListePatientsGlobal = new List<Patient>();
    private readonly PatientService _patientService = new PatientService();

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            ListePatientsGlobal = _patientService.ObtenirTousLesPatients();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des données depuis PostgreSQL : {ex.Message}", 
                            "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        RafraichirTableau();
    }

    // Cette fonction sert à forcer l'affichage à se recharger
    public void RafraichirTableau()
    {
        dgPatients.ItemsSource = null;
        dgPatients.ItemsSource = ListePatientsGlobal;
    }

    // Le bouton de recherche là...
    //  Faudrait rajouter un bouton pour réinitialier mais j'ai la flemme
    private void btnRecherche_Click(object sender, RoutedEventArgs e)
    {
        string filtre = txtRecherche.Text.ToLower().Trim();
        
        if (string.IsNullOrWhiteSpace(filtre))
        {
            dgPatients.ItemsSource = ListePatientsGlobal;
        }
        else
        {
            var resultat = ListePatientsGlobal.FindAll(p => 
                p.NumeroDossier.ToLower().Contains(filtre) || 
                p.Nom.ToLower().Contains(filtre) || 
                p.Prenom.ToLower().Contains(filtre));
            dgPatients.ItemsSource = resultat;
        }
    }

    // Double click pour ouvrir les info du patient. Trop fier de ça aussi XD
    private void dgPatients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (dgPatients.SelectedItem is Patient patientSelectionne)
        {
            DetailPatientWindow detailWindow = new DetailPatientWindow(patientSelectionne);
            detailWindow.ShowDialog();
        }
    }
}