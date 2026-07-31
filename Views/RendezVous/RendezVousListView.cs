using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;

public partial class RendezVousListView : UserControl
{
    private readonly RendezVousService _rendezVousService = new();

    public RendezVousListView()
    {
        InitializeComponent();
        RafraichirGrille();
    }

    private void Filtre_Changed(object sender, RoutedEventArgs e) => RafraichirGrille();

    private void RafraichirGrille()
    {
        // Les evenements de filtre (SelectionChanged sur cbStatut,
        // notamment) peuvent se declencher pendant le chargement du
        // XAML lui-meme, avant que dgRendezVous (declare plus bas dans
        // le fichier) ne soit pret. Dans ce cas, on ne fait rien : la
        // grille sera de toute facon remplie juste apres, dans le
        // constructeur.
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
        }
        catch (Exception ex)
        {
            txtMessage.Foreground = Brushes.Red;
            txtMessage.Text = $"Erreur lors du chargement des rendez-vous : {ex.Message}";
        }
    }

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

    // Ouvre la fenetre de detail du rendez-vous, sur le meme principe
    // que DetailPatientWindow / DetailMedecinWindow.
    private void dgRendezVous_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (dgRendezVous.SelectedItem is not RendezVousAffichage ligne) return;

        var detail = _rendezVousService.ObtenirDetail(ligne.NumeroRdv);
        if (detail is null) return;

        var fenetre = new RendezVousDetailWindow(detail);
        fenetre.ShowDialog();
        RafraichirGrille();
    }

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
    {
        var txtMotif = new TextBox { Height = 32, Padding = new Thickness(8, 6, 8, 6), Margin = new Thickness(0, 8, 0, 16) };
        var boutonValider = new Button { Content = "Confirmer l'annulation", Background = Brushes.IndianRed, Foreground = Brushes.White, Padding = new Thickness(10, 6, 10, 6), BorderThickness = new Thickness(0) };
        var panneau = new StackPanel { Margin = new Thickness(16) };
        panneau.Children.Add(new TextBlock { Text = "Motif de l'annulation :", FontSize = 12, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 4) });
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

    private void btnReprogrammer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroRdv) return;

        var ligne = (dgRendezVous.ItemsSource as System.Collections.Generic.List<RendezVousAffichage>)?
            .FirstOrDefault(r => r.NumeroRdv == numeroRdv);
        if (ligne is null) return;

        var nouvelleDateHeure = DemanderNouvelleDateHeure(ligne.DateHeure);
        if (nouvelleDateHeure is null) return;

        try
        {
            _rendezVousService.ReprogrammerRendezVous(numeroRdv, nouvelleDateHeure.Value);
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

    // Petite fenetre avec un vrai selecteur date + heure (remplace
    // l'ancienne version simplifiee qui ajoutait juste 1 jour).
    private DateTime? DemanderNouvelleDateHeure(DateTime dateActuelle)
    {
        var dp = new DatePicker { SelectedDate = dateActuelle.Date, Height = 32, Margin = new Thickness(0, 4, 0, 12) };
        var txtHeure = new TextBox { Text = dateActuelle.ToString("HH:mm"), Height = 32, Padding = new Thickness(8, 6, 8, 6), Margin = new Thickness(0, 4, 0, 16) };
        var boutonValider = new Button { Content = "Confirmer la reprogrammation", Background = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)), Foreground = Brushes.White, Padding = new Thickness(10, 6, 10, 6), BorderThickness = new Thickness(0) };
        var txtErreur = new TextBlock { Foreground = Brushes.Red, FontSize = 11, Margin = new Thickness(0, 8, 0, 0) };

        var panneau = new StackPanel { Margin = new Thickness(16) };
        panneau.Children.Add(new TextBlock { Text = "Nouvelle date :", FontSize = 12, Foreground = Brushes.Gray });
        panneau.Children.Add(dp);
        panneau.Children.Add(new TextBlock { Text = "Nouvelle heure (HH:mm) :", FontSize = 12, Foreground = Brushes.Gray });
        panneau.Children.Add(txtHeure);
        panneau.Children.Add(boutonValider);
        panneau.Children.Add(txtErreur);

        var fenetre = new Window
        {
            Title = "Reprogrammer le rendez-vous",
            Content = panneau,
            Width = 360,
            Height = 260,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            ResizeMode = ResizeMode.NoResize
        };

        DateTime? resultat = null;
        boutonValider.Click += (_, _) =>
        {
            if (dp.SelectedDate is null || !TimeSpan.TryParse(txtHeure.Text, out var heure))
            {
                txtErreur.Text = "Date ou heure invalide.";
                return;
            }
            resultat = dp.SelectedDate.Value.Date + heure;
            fenetre.Close();
        };
        fenetre.ShowDialog();
        return resultat;
    }
}