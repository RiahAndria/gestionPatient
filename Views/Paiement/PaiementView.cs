using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Paiement;

// Utilise pour cacher le bouton "Facturer" une fois EstFacture = true.
public class BoolInverseToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public partial class PaiementView : UserControl
{
    private readonly PaiementService _paiementService = new();
    private readonly RappelService _rappelService = new();

    public PaiementView()
    {
        InitializeComponent();
        Rafraichir();
    }

    public void Rafraichir()
    {
        try
        {
            dgIncomplets.ItemsSource = _paiementService.ObtenirPaiementsIncomplets();
            dgHistorique.ItemsSource = _paiementService.ObtenirHistoriquePayes();
        }
        catch (Exception ex)
        {
            AfficherMessage($"Erreur lors du chargement des paiements : {ex.Message}", succes: false);
        }
    }

    private void btnAlertePaiement_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroRdv) return;

        try
        {
            _rappelService.CreerAlertePaiement(numeroRdv);
            AfficherMessage($"Alerte de paiement envoyée pour le rendez-vous {numeroRdv}.", succes: true);
        }
        catch (Exception ex)
        {
            AfficherMessage($"Impossible de créer l'alerte : {ex.Message}", succes: false);
        }

        Rafraichir();
    }

    private void btnRegler_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not PaiementIncomplet ligne) return;

        var fenetre = new ReglerSoldeWindow(ligne) { Owner = Window.GetWindow(this) };
        if (fenetre.ShowDialog() == true)
        {
            AfficherMessage($"Paiement réglé pour {ligne.PatientNom}.", succes: true);
        }

        Rafraichir();
    }

    private void btnRejeter_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not PaiementIncomplet ligne) return;

        var fenetre = new RejeterPaiementWindow(ligne) { Owner = Window.GetWindow(this) };
        if (fenetre.ShowDialog() == true)
        {
            AfficherMessage($"Paiement rejeté pour {ligne.PatientNom}.", succes: true);
        }

        Rafraichir();
    }

    private void btnFacturer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroPaiement) return;

        var detail = _paiementService.ObtenirDetailFacture(numeroPaiement);
        if (detail is null)
        {
            AfficherMessage("Impossible de charger les détails de ce paiement.", succes: false);
            return;
        }

        var fenetre = new FactureWindow(detail) { Owner = Window.GetWindow(this) };
        fenetre.ShowDialog();
        Rafraichir();
    }

    private void AfficherMessage(string message, bool succes)
    {
        txtMessage.Foreground = succes ? new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)) : Brushes.Red;
        txtMessage.Text = message;
    }
}
