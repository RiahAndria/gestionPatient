using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Services;

namespace Patients.Views.Paiement;

public partial class PaiementView : UserControl
{
    private readonly PaiementService _paiementService = new();

    public PaiementView()
    {
        InitializeComponent();
        Rafraichir();
    }

    public void Rafraichir()
    {
        try
        {
            cbRdvAcompte.ItemsSource = _paiementService.ObtenirRendezVousEligiblesAcompte();
            dgEnAttente.ItemsSource = _paiementService.ObtenirEnAttente();
            dgHistorique.ItemsSource = _paiementService.ObtenirHistoriquePayes();
        }
        catch (Exception ex)
        {
            AfficherMessage($"Erreur lors du chargement des paiements : {ex.Message}", succes: false);
        }
    }

    private void btnEncaisserAcompte_Click(object sender, RoutedEventArgs e)
    {
        if (cbRdvAcompte.SelectedItem is not Models.RendezVousAffichage rdv)
        {
            AfficherMessage("Sélectionne un rendez-vous.", succes: false);
            return;
        }
        if (!decimal.TryParse(txtMontantAcompte.Text, out var montant) || montant <= 0)
        {
            AfficherMessage("Renseigne un montant d'acompte valide.", succes: false);
            return;
        }

        var mode = (cbModeAcompte.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Espèces";

        try
        {
            var resultat = _paiementService.EncaisserAcompte(rdv.NumeroRdv, montant, mode);

            if (!resultat.Succes)
            {
                // Cas 1 : le montant depasse le tarif du medecin - refuse.
                AfficherMessage(resultat.MessageErreur ?? "Montant invalide.", succes: false);
                return;
            }

            if (resultat.PaiementComplet)
            {
                // Cas 2 : le total verse atteint exactement le tarif - regle en entier.
                AfficherMessage($"Paiement complet enregistré pour {rdv.PatientNom} ({montant:N0} Ar). Ce rendez-vous est intégralement réglé.", succes: true);
            }
            else
            {
                // Cas 3 : le total verse est encore inferieur au tarif - acompte partiel.
                AfficherMessage($"Acompte de {montant:N0} Ar encaissé pour {rdv.PatientNom}. Reste à payer : {resultat.MontantRestant:N0} Ar (facturé automatiquement après la consultation).", succes: true);
            }

            txtMontantAcompte.Clear();
        }
        catch (Exception ex)
        {
            AfficherMessage($"Impossible d'encaisser l'acompte : {ex.Message}", succes: false);
        }

        Rafraichir();
    }

    private void btnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroPaiement) return;

        try
        {
            _paiementService.ConfirmerPaiement(numeroPaiement, "Espèces");
            AfficherMessage($"Paiement {numeroPaiement} confirmé.", succes: true);
        }
        catch (Exception ex)
        {
            AfficherMessage($"Impossible de confirmer : {ex.Message}", succes: false);
        }

        Rafraichir();
    }

    private void btnRelancer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroPaiement) return;

        try
        {
            var numeroRelance = _paiementService.EnvoyerRelance(numeroPaiement);
            AfficherMessage($"Relance n°{numeroRelance} envoyée pour {numeroPaiement}.", succes: true);
        }
        catch (Exception ex)
        {
            AfficherMessage($"Impossible d'envoyer la relance : {ex.Message}", succes: false);
        }

        Rafraichir();
    }

    private void btnTraiterImpayes_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var actions = _paiementService.TraiterImpayes();
            AfficherMessage(string.Join("\n", actions), succes: true);
        }
        catch (Exception ex)
        {
            AfficherMessage($"Erreur lors du traitement des impayés : {ex.Message}", succes: false);
        }

        Rafraichir();
    }

    private void AfficherMessage(string message, bool succes)
    {
        txtMessage.Foreground = succes ? new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)) : Brushes.Red;
        txtMessage.Text = message;
    }
}