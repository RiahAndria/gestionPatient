using System.Collections.Generic;
using System.Windows;
using Patients.Models;

namespace Patients;

public partial class MainWindow : Window
{
    public static List<Patient> ListePatientsGlobal = new List<Patient>();

    public MainWindow()
    {
        InitializeComponent();
        SetupNavigation();
    }

    private void SetupNavigation()
    {
        // Relie les RadioButtons du composant SidebarView aux onglets du TabControl
        SidebarNav.PatientsButton.Checked += (s, e) => MainTabControl.SelectedItem = TabPatients;
        SidebarNav.MedecinsButton.Checked += (s, e) => MainTabControl.SelectedItem = TabMedecins;
        SidebarNav.DossiersButton.Checked += (s, e) => MainTabControl.SelectedItem = TabDossiers;
        SidebarNav.HistoriqueButton.Checked += (s, e) => MainTabControl.SelectedItem = TabHistorique;
        SidebarNav.RendezVousButton.Checked += (s, e) => MainTabControl.SelectedItem = TabRendezVous;
    }

    public void RefreshPatientList()
    {
        PatientList?.ChargerDonnees();
    }
}
