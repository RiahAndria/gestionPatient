using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Services;

namespace Patients.Views.Paiement;
//création de la vue pour le paiement
public partial class PaiementView : UserControl
{// Service pour gérer les paiements
    private readonly PaiementService _paiementService = new();
// Constructeur de la vue
    public PaiementView()
    {
        InitializeComponent();
        Rafraichir();
    }
// Méthode pour rafraîchir les données affichées dans la vue
    private void Rafraichir()
    {
        try
        {
            dgEnAttente.ItemsSource = _paiementService.ObtenirEnAttente();
            dgHistorique.ItemsSource = _paiementService.ObtenirHistoriquePayes();
        }
        catch (Exception ex)
        {
            AfficherMessage($"Erreur lors du chargement des paiements : {ex.Message}", succes: false);
        }
    }
// Méthode pour gérer le clic sur le bouton "Confirmer"
    private void btnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string numeroPaiement) return;

        try
        {
            // Simplification maquette : mode de paiement fixe a "Espèces"
            _paiementService.ConfirmerPaiement(numeroPaiement, "Espèces");
            AfficherMessage($"Paiement {numeroPaiement} confirmé.", succes: true);
        }
        catch (Exception ex)
        {
            AfficherMessage($"Impossible de confirmer : {ex.Message}", succes: false);
        }

        Rafraichir();
    }
// Méthode pour gérer le clic sur le bouton "Relancer"
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
// Méthode pour gérer le clic sur le bouton "Traiter impayés"
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
// Méthode pour afficher les messages d'erreur ou de succès
    private void AfficherMessage(string message, bool succes)
    {
        txtMessage.Foreground = succes ? Brushes.Green : Brushes.Red;
        txtMessage.Text = message;
    }
}