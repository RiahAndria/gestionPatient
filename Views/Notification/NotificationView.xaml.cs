using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

    public NotificationView()
    {
        InitializeComponent();
        Rafraichir();
    }

    public void Rafraichir()
    {
        var notifications = _rappelService.ObtenirToutesLesNotifications();
        ListeNotifications.ItemsSource = notifications;
        TxtAucune.Visibility = notifications.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        int nbNonLues = notifications.Count(n => !n.Lu);
        TxtTitre.Text = nbNonLues > 0 ? $"Notifications ({nbNonLues} non lue(s))" : "Notifications";
    }

    private void BtnGenererRappels_Click(object sender, RoutedEventArgs e)
    {
        int nb = _rappelService.GenererRappels24h();
        MessageBox.Show($"{nb} nouvelle(s) notification(s) générée(s) pour les rendez-vous des prochaines 24h.",
                        "Rappels RDV", MessageBoxButton.OK, MessageBoxImage.Information);
        Rafraichir();
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

    // Double-clic (ou simple clic, ici) sur une notification : ouvre le
    // detail du rendez-vous concerne, comme partout ailleurs dans
    // l'application, et marque la notification comme lue au passage.
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
