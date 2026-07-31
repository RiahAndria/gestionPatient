using System.Collections.Generic;
using System.Windows;
using Patients.Models;
using Patients.Services;
namespace Patients;

public partial class MainWindow : Window
{
    public static List<Patients.Models.Patient> ListePatientsGlobal = new List<Patients.Models.Patient>();
    private readonly PatientService _patientService = new PatientService();
    // public static List<Patient> ListePatientsGlobal = new List<Patient>();

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

    // Double click pour ouvrir les info du patient. Trop fier de ça aussi XD
    // private void dgPatients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    // {
    //     if (dgPatients.SelectedItem is Patients.Models.Patient patientSelectionne)
    //     {
    //         DetailPatientWindow detailWindow = new DetailPatientWindow(patientSelectionne);
    //         detailWindow.ShowDialog();
    //     }
    // }
}
