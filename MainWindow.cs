using System.Windows;
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
    SidebarNav.ConsultationsButton.Checked += (s, e) => MainTabControl.SelectedItem = TabConsultations;
    SidebarNav.RendezVousButton.Checked += (s, e) => { MainTabControl.SelectedItem = TabRendezVous; VueRendezVous.RafraichirGrille(); };
    SidebarNav.PaiementsButton.Checked += (s, e) => { MainTabControl.SelectedItem = TabPaiements; VuePaiements.Rafraichir(); };
    SidebarNav.NotificationsButton.Checked += (s, e) =>
    {
        MainTabControl.SelectedItem = TabNotifications;
        VueNotifications.Rafraichir();
        SidebarNav.RafraichirBadgeNotifications();
    };
}
    public void RefreshPatientList()
    {
        PatientList?.ChargerDonnees();
    }
}