using System.Windows;
using System.Windows.Controls;
using Patients.Views.Paiement;

namespace Patients.Views.RendezVous;
//création de la vue pour le module des rendez-vous
public partial class RendezVousModuleView : UserControl
{
    private RendezVousListView? _vueRendezVous;
    private PaiementView? _vuePaiement;
// Constructeur de la vue
    public RendezVousModuleView()
    {
        InitializeComponent();
        ongletRendezVous.IsChecked = true;
    }

    private void ongletRendezVous_Checked(object sender, RoutedEventArgs e) => AfficherRendezVous();

    private void ongletPaiements_Checked(object sender, RoutedEventArgs e) => AfficherPaiements();
// Méthode pour afficher la vue des rendez-vous
    private void AfficherRendezVous()
    {
        _vueRendezVous ??= new RendezVousListView();
        contenuOnglet.Content = _vueRendezVous;
    }
// Méthode pour afficher la vue des paiements
    private void AfficherPaiements()
    {
        _vuePaiement ??= new PaiementView();
        contenuOnglet.Content = _vuePaiement;
    }
}