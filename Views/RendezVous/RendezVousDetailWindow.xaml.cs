using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;

public partial class RendezVousDetailWindow : Window
{
    private readonly PaiementService _paiementService = new();
    private readonly RendezVousService _rendezVousService = new();
    private string _numeroRdv = "";

    public RendezVousDetailWindow(RendezVousDetail detail)
    {
        InitializeComponent();
        _numeroRdv = detail.NumeroRdv;
        Charger(detail);
    }

    // Recharge tout l'affichage depuis un RendezVousDetail deja
    // recupere (evite une requete en plus au premier affichage).
    private void Charger(RendezVousDetail detail)
    {
        lblNumeroRdv.Text = $"RENDEZ-VOUS {detail.NumeroRdv}";
        lblStatut.Text = detail.StatutAffiche;
        badgeStatut.Background = detail.Statut switch
        {
            "PLANIFIE" => new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)),
            "ANNULE" => new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)),
            "TERMINE" => new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)),
            _ => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
        };

        lblDateHeure.Text = detail.DateHeure.ToString("dddd dd MMMM yyyy 'à' HH:mm");
        lblMotif.Text = $"Motif : {detail.Motif}";
        lblMotifAnnulation.Text = string.IsNullOrWhiteSpace(detail.MotifAnnulation)
            ? ""
            : $"Annulation : {detail.MotifAnnulation}";

        lblPatientNom.Text = detail.PatientNom;
        lblPatientMatricule.Text = $"Dossier n° {detail.PatientMatricule}";
        lblPatientContact.Text = $"{detail.PatientTelephone}\n{detail.PatientEmail}";

        lblMedecinNom.Text = detail.MedecinNom;
        lblMedecinFonction.Text = $"{detail.MedecinFonction} — {detail.MedecinTauxHoraire:N0} Ar/consultation";

        ChargerPaiement(detail);
    }

    // Le detail du paiement n'est pas stocke dans RendezVousDetail : on
    // le recalcule ici a partir des paiements lies au RDV plus le tarif
    // du medecin, deja disponibles via PaiementService.
    private void ChargerPaiement(RendezVousDetail detail)
    {
        var paiements = _paiementService.ObtenirParRendezVous(detail.NumeroRdv);

        decimal montantTotal = detail.MedecinTauxHoraire;
        decimal montantPaye = paiements.Where(p => p.EstPaye).Sum(p => p.Montant);
        decimal montantNonPaye = System.Math.Max(0, montantTotal - montantPaye);

        lblMontantTotal.Text = $"{montantTotal:N0} Ar";
        lblMontantPaye.Text = $"{montantPaye:N0} Ar";
        lblMontantNonPaye.Text = $"{montantNonPaye:N0} Ar";

        // Date limite = date du rendez-vous, uniquement pertinente s'il
        // reste un solde a payer (paiement en avance incomplet).
        ligneDateLimite.Visibility = montantNonPaye > 0 ? Visibility.Visible : Visibility.Collapsed;
        lblDateLimite.Text = detail.DateHeure.ToString("dd/MM/yyyy");

        var dernierMode = paiements.OrderByDescending(p => p.DateFacture).FirstOrDefault()?.ModePaiement;
        lblModePaiement.Text = dernierMode ?? "—";

        lblAucunPaiement.Visibility = paiements.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RafraichirDepuisBase()
    {
        var detail = _rendezVousService.ObtenirDetail(_numeroRdv);
        if (detail != null) Charger(detail);
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnReprogrammer_Click(object sender, RoutedEventArgs e)
    {
        var detail = _rendezVousService.ObtenirDetail(_numeroRdv);
        if (detail is null) return;

        var nouvelleDateHeure = DemanderNouvelleDateHeure(detail.DateHeure);
        if (nouvelleDateHeure is null) return;

        try
        {
            _rendezVousService.ReprogrammerRendezVous(_numeroRdv, nouvelleDateHeure.Value);
            RafraichirDepuisBase();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Impossible de reprogrammer : {ex.Message}", "Reprogrammer", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnChangerStatut_Click(object sender, RoutedEventArgs e)
    {
        var detail = _rendezVousService.ObtenirDetail(_numeroRdv);
        if (detail is null) return;

        var fenetre = new ChangerStatutWindow(detail.Statut) { Owner = this };
        if (fenetre.ShowDialog() == true && fenetre.StatutChoisi != null)
        {
            try
            {
                _rendezVousService.ChangerStatut(_numeroRdv, fenetre.StatutChoisi);
                RafraichirDepuisBase();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Impossible de changer le statut : {ex.Message}", "Changer statut", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            $"Supprimer définitivement le rendez-vous {_numeroRdv} ?\nCette action est irréversible et supprime aussi les paiements et notifications liés.",
            "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            _rendezVousService.SupprimerDefinitivement(_numeroRdv);
            Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Impossible de supprimer : {ex.Message}", "Supprimer", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Petite fenetre avec un vrai selecteur date + heure.
    private DateTime? DemanderNouvelleDateHeure(DateTime dateActuelle)
    {
        var dp = new DatePicker { SelectedDate = dateActuelle.Date, Height = 32, Margin = new Thickness(0, 4, 0, 12) };
        var txtHeure = new TextBox { Text = dateActuelle.ToString("HH:mm"), Height = 32, Padding = new Thickness(8, 6, 8, 6), Margin = new Thickness(0, 4, 0, 16) };
        var boutonValider = new Button { Content = "Confirmer la reprogrammation", Background = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)), Foreground = Brushes.White, Padding = new Thickness(10, 6, 10, 6), BorderThickness = new Thickness(0) };
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
            Owner = this,
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
