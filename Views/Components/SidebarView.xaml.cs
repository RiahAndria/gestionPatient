using System.Windows.Controls;

namespace Patients.Views.Components;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
    }

    // Expose les RadioButtons pour le Binding ou l'interaction directe depuis MainWindow
    public RadioButton PatientsButton => NavPatients;
    public RadioButton MedecinsButton => NavMedecins;
    public RadioButton DossiersButton => NavDossiers;
    public RadioButton HistoriqueButton => NavHistorique;
    public RadioButton RendezVousButton => NavRendezVous;
    public RadioButton PaiementsButton => NavPaiements;
}
