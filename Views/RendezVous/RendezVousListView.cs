//on affiche la liste des rendez-vous avec possibilité de recherche, filtrage et actions sur les rendez-vous
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;
//création de la vue pour la liste des rendez-vous
public partial class RendezVousListView : UserControl
{
    private readonly RendezVousService _rendezVousService = new();
// Constructeur de la vue
    public RendezVousListView()
    {
        InitializeComponent();
        RafraichirGrille();
    }
// Méthode pour gérer le clic sur le bouton "Rechercher"
    private void RafraichirGrille()
    {
        string terme = txtRecherche?.Text?.Trim() ?? "";
        DateTime? date = dpDate?.SelectedDate;
        string statutFiltre = (cbStatut?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tous";
        string statutRequete = statutFiltre switch
        {
            "Planifie" => "PLANIFIE",
            "Annule" => "ANNULE",
            "Termine" => "TERMINE",
            _ => ""
        };
// On essaie de récupérer les rendez-vous en fonction des critères de recherche et on les affiche dans la grille
        try
        {
            var lignes = _rendezVousService.Rechercher(terme, date, statutRequete);
            dgRendezVous.ItemsSource = lignes;
        }
        // On gère les exceptions qui peuvent survenir lors de la récupération des rendez-vous
        catch (Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Erreur lors du chargement des rendez-vous : {ex.Message}";
        }
    }
// Méthode pour gérer le clic sur le bouton "Nouveau"
    private void btnNouveau_Click(object sender, RoutedEventArgs e)
    {
        var fenetre = new Window
        {
            Title = "Nouveau rendez-vous",
            Content = new RendezVousFormView(),
            Width = 480,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this)
        };
        fenetre.ShowDialog();
        RafraichirGrille();
    }
// Méthode pour gérer le clic sur le bouton "Rechercher"
    private void dgRendezVous_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (dgRendezVous.SelectedItem is not RendezVousAffichage rendezVous) return;

        var fenetre = new Window
        {
            Title = $"Détails du rendez-vous {rendezVous.NumeroRdv}",
            Content = new RendezVousFormView(),
            Width = 480,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this)
        };
        fenetre.ShowDialog();
        RafraichirGrille();
    }
// Méthode pour gérer le clic sur le bouton "Annuler"
    private void btnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroRdv) return;

        var motif = DemanderMotifAnnulation();
        if (string.IsNullOrWhiteSpace(motif)) return;

        try
        {
            _rendezVousService.AnnulerRendezVous(numeroRdv, motif);
            txtMessage.Foreground = Brushes.Green;
            txtMessage.Text = $"Rendez-vous {numeroRdv} annulé.";
        }
        catch (Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Impossible d'annuler : {ex.Message}";
        }

        RafraichirGrille();
    }
    private string? DemanderMotifAnnulation()
    //méthode pour demander le motif d'annulation du rendez-vous
    {
        var txtMotif = new TextBox { Style = (Style)FindResource("ChampSaisie"), Margin = new Thickness(0, 8, 0, 16) };
        var boutonValider = new Button { Content = "Confirmer l'annulation", Style = (Style)FindResource("BoutonDanger") };
        var panneau = new StackPanel { Margin = new Thickness(16) };
        panneau.Children.Add(new TextBlock { Text = "Motif de l'annulation :", Style = (Style)FindResource("Libelle") });
        panneau.Children.Add(txtMotif);
        panneau.Children.Add(boutonValider);

        var fenetre = new Window
        {
            Title = "Annuler le rendez-vous",
            Content = panneau,
            Width = 360,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            ResizeMode = ResizeMode.NoResize
        };

        string? resultat = null;
        boutonValider.Click += (_, _) => { resultat = txtMotif.Text; fenetre.Close(); };
        fenetre.ShowDialog();
        return resultat;
    }
// Méthode pour gérer le clic sur le bouton "Reprogrammer"
    private void btnReprogrammer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroRdv) return;

        var ligne = (dgRendezVous.ItemsSource as System.Collections.Generic.List<RendezVousAffichage>)?
            .FirstOrDefault(r => r.NumeroRdv == numeroRdv);
        if (ligne is null) return;

        try
        {
            _rendezVousService.ReprogrammerRendezVous(numeroRdv, ligne.DateHeure.AddDays(1));
            txtMessage.Foreground = Brushes.DarkOrange;
            txtMessage.Text = $"Rendez-vous {numeroRdv} reprogrammé.";
        }
        catch (Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Impossible de reprogrammer : {ex.Message}";
        }

        RafraichirGrille();
    }
}