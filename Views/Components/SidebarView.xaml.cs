using System.Windows;
using System.Windows.Controls;
using Patients.Services;

namespace Patients.Views.Components;

public partial class SidebarView : UserControl
{
    private readonly RappelService _rappelService = new();

    public SidebarView()
    {
        InitializeComponent();
        RafraichirBadgeNotifications();
    }

    // Expose les RadioButtons pour le Binding ou l'interaction directe depuis MainWindow
    public RadioButton PatientsButton => NavPatients;
    public RadioButton MedecinsButton => NavMedecins;
    public RadioButton ConsultationsButton => NavConsultations;
    public RadioButton RendezVousButton => NavRendezVous;
    public RadioButton PaiementsButton => NavPaiements;
    public RadioButton NotificationsButton => NavNotifications;

    // Appelee au demarrage, et a rappeler depuis MainWindow chaque fois
    // qu'une notification est generee/lue ailleurs, pour que le badge
    // reste juste sans avoir a relancer l'application.
    public void RafraichirBadgeNotifications()
    {
        int nbNonLues = _rappelService.CompterNonLues();

        if (nbNonLues > 0)
        {
            BadgeNotif.Visibility = Visibility.Visible;
            TxtBadgeNotif.Text = nbNonLues > 9 ? "9+" : nbNonLues.ToString();
        }
        else
        {
            BadgeNotif.Visibility = Visibility.Collapsed;
        }
    }
}
