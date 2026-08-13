using System;
using System.Windows;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Paiement;

public partial class RejeterPaiementWindow : Window
{
    private readonly PaiementService _paiementService = new();
    private readonly RendezVousService _rendezVousService = new();
    private readonly PaiementIncomplet _ligne;

    public RejeterPaiementWindow(PaiementIncomplet ligne)
    {
        InitializeComponent();
        _ligne = ligne;

        lblPatient.Text = ligne.PatientNom;
        lblContexte.Text = ligne.EstAcompteEnAttente
            ? $"Rejeter le solde en attente sur le rendez-vous {ligne.NumeroRdv} annulera ce rendez-vous."
            : $"Rejeter la facture {ligne.NumeroPaiement} la supprimera définitivement.";
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        string motif = txtMotif.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(motif))
        {
            txtErreur.Text = "Indique un motif de rejet.";
            return;
        }

        var confirmation = MessageBox.Show(
            "Cette action est irréversible. Confirmer le rejet ?",
            "Confirmer le rejet", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            if (_ligne.EstAcompteEnAttente)
            {
                // Il n'existe pas de ligne PAIEMENT dediee pour un solde
                // pas encore facture : rejeter revient a annuler le
                // rendez-vous concerne (rien d'autre a "supprimer").
                _rendezVousService.AnnulerRendezVous(_ligne.NumeroRdv, $"Paiement rejeté : {motif}");
            }
            else
            {
                _paiementService.RejeterPaiement(_ligne.NumeroPaiement!, motif);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            txtErreur.Text = $"Erreur : {ex.Message}";
        }
    }
}
