using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;

public partial class RendezVousDetailWindow : Window
{
    private readonly PaiementService _paiementService = new();

    public RendezVousDetailWindow(RendezVousDetail detail)
    {
        InitializeComponent();

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

        var paiements = _paiementService.ObtenirParRendezVous(detail.NumeroRdv);
        listePaiements.ItemsSource = paiements;
        lblAucunPaiement.Visibility = paiements.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e) => Close();
}
