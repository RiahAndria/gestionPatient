using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;

public partial class RendezVousListView : UserControl
{
    private readonly RendezVousService _rendezVousService = new();
    private readonly RappelService _rappelService = new();

    public RendezVousListView()
    {
        InitializeComponent();
        RafraichirGrille();
    }

    // Recherche declenchee uniquement par le bouton "Rechercher" (plus
    // de filtrage en direct a chaque frappe), comme demande.
    private void btnRechercher_Click(object sender, RoutedEventArgs e) => RafraichirGrille();

    // Reinitialise les champs de recherche et revient a la liste complete.
    private void btnRetour_Click(object sender, RoutedEventArgs e)
    {
        txtRecherche.Text = "";
        dpDate.SelectedDate = null;
        cbStatut.SelectedIndex = 0;
        RafraichirGrille();
    }

    public void RafraichirGrille()
    {
        if (dgRendezVous is null) return;

        string terme = txtRecherche?.Text?.Trim() ?? "";
        DateTime? date = dpDate?.SelectedDate;
        string statutFiltre = (cbStatut?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tous";

        string statutRequete = statutFiltre switch
        {
            "Planifié" => "PLANIFIE",
            "Annulé" => "ANNULE",
            "Terminé" => "TERMINE",
            _ => ""
        };

        try
        {
            dgRendezVous.ItemsSource = _rendezVousService.Rechercher(terme, date, statutRequete);
            txtMessage.Text = "";
        }
        catch (System.Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Erreur lors du chargement des rendez-vous : {ex.Message}";
        }
    }

    // Ouvre l'assistant de creation de rendez-vous en 7 etapes (voir
    // Views/RendezVous/Assistant/).
    private void btnNouveau_Click(object sender, RoutedEventArgs e)
    {
        var fenetre = new Assistant.NouveauRendezVousWindow
        {
            Owner = Window.GetWindow(this)
        };
        fenetre.ShowDialog();
        RafraichirGrille();
    }

    // Ouvre la fenetre de detail du rendez-vous (Reprogrammer / Changer
    // statut / Supprimer / Fermer y sont geres directement).
    private void dgRendezVous_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (dgRendezVous.SelectedItem is not RendezVousAffichage ligne) return;

        var detail = _rendezVousService.ObtenirDetail(ligne.NumeroRdv);
        if (detail is null) return;

        var fenetre = new RendezVousDetailWindow(detail);
        fenetre.ShowDialog();
        RafraichirGrille();
    }

    // Bouton 🔔 de la colonne Alertes : cree une alerte de rappel pour
    // ce rendez-vous (envoyee vers la page Notifications, onglet
    // Réservations) et rafraichit le compteur affiche.
    private void btnAlerte_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroRdv) return;

        try
        {
            _rappelService.CreerAlerteRendezVous(numeroRdv);
            txtMessage.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));
            txtMessage.Text = $"Alerte envoyée pour le rendez-vous {numeroRdv}.";
        }
        catch (System.Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Impossible de créer l'alerte : {ex.Message}";
        }

        RafraichirGrille();
    }
}
