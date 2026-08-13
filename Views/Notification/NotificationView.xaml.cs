using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;
using Patients.Views.RendezVous;

namespace Patients.Views.Notification;

// Utilise uniquement pour cacher le bouton "Marquer comme lue" quand
// la notification est deja lue (evite d'ajouter une dependance externe
// pour un besoin aussi simple).
public class BoolInverseToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public partial class NotificationView : UserControl
{
    private readonly RappelService _rappelService = new();
    private readonly RendezVousService _rendezVousService = new();

    // Onglet actif : "RESERVATION" ou "PAIEMENT".
    private string _typeActif = "RESERVATION";

    private static readonly SolidColorBrush _brushOngletActif = new(Color.FromRgb(0x25, 0x63, 0xEB));
    private static readonly SolidColorBrush _brushOngletInactif = new(Color.FromRgb(0xF1, 0xF5, 0xF9));

    public NotificationView()
    {
        InitializeComponent();
        MettreAJourOnglets();
        Rafraichir();
    }

    private void BtnOngletReservations_Click(object sender, RoutedEventArgs e)
    {
        _typeActif = "RESERVATION";
        MettreAJourOnglets();
        Rafraichir();
    }

    private void BtnOngletPaiements_Click(object sender, RoutedEventArgs e)
    {
        _typeActif = "PAIEMENT";
        MettreAJourOnglets();
        Rafraichir();
    }

    private void MettreAJourOnglets()
    {
        bool reservationsActif = _typeActif == "RESERVATION";
        BtnOngletReservations.Background = reservationsActif ? _brushOngletActif : _brushOngletInactif;
        BtnOngletReservations.Foreground = reservationsActif ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));
        BtnOngletPaiements.Background = !reservationsActif ? _brushOngletActif : _brushOngletInactif;
        BtnOngletPaiements.Foreground = !reservationsActif ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));
    }

    public void Rafraichir()
    {
        var notifications = _rappelService.ObtenirNotifications(_typeActif);
        ListeNotifications.ItemsSource = notifications;
        TxtAucune.Visibility = notifications.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        int nbNonLues = notifications.Count(n => !n.Lu);
        string libelleOnglet = _typeActif == "RESERVATION" ? "Réservations" : "Paiements";
        TxtTitre.Text = nbNonLues > 0 ? $"Notifications — {libelleOnglet} ({nbNonLues} non lue(s))" : $"Notifications — {libelleOnglet}";
    }

    private void BtnMarquerLue_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroNotif) return;
        _rappelService.MarquerCommeLue(numeroNotif);
        Rafraichir();
    }

    private void BtnMarquerToutesLues_Click(object sender, RoutedEventArgs e)
    {
        _rappelService.MarquerToutesCommeLues();
        Rafraichir();
    }

    // Ouvre le detail du rendez-vous concerne (les 2 types de
    // notification y renvoient : la section Paiement de cette fenetre
    // couvre aussi les infos de paiement) et marque la notification
    // comme lue au passage.
    private void Notification_Click(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not Models.Notification notif) return;

        _rappelService.MarquerCommeLue(notif.NumeroNotif);

        var detail = _rendezVousService.ObtenirDetail(notif.NumeroRdv);
        if (detail != null)
        {
            var fenetre = new RendezVousDetailWindow(detail);
            fenetre.ShowDialog();
        }

        Rafraichir();
    }
}
