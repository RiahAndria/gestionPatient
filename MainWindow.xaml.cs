using System.Collections.Generic;
using System.Windows;
using Patients.Models;
using Patients.Services;
namespace Patients;

public partial class MainWindow : Window
{
    public static List<Patients.Models.Patient> ListePatientsGlobal = new List<Patients.Models.Patient>();
    private readonly PatientService _patientService = new PatientService();

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
        SidebarNav.PaiementsButton.Checked += (s, e) => MainTabControl.SelectedItem = TabPaiements;
    }

    public void RefreshPatientList()
    {
        PatientList?.ChargerDonnees();
    }

}
