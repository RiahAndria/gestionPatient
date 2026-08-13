using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Paiement;

// Fenetre de reglement, ouverte depuis la section "Paiements non
// complets" (bouton "Régler"). Reprend la meme logique de validation
// que l'etape 6 de l'assistant de rendez-vous (PaiementService).
public partial class ReglerSoldeWindow : Window
{
    private readonly PaiementService _paiementService = new();
    private readonly PaiementIncomplet _ligne;

    public ReglerSoldeWindow(PaiementIncomplet ligne)
    {
        InitializeComponent();
        _ligne = ligne;

        lblPatient.Text = ligne.PatientNom;
        lblContexte.Text = ligne.EstAcompteEnAttente
            ? $"Solde restant sur l'avance déjà versée pour le rendez-vous {ligne.NumeroRdv}."
            : $"Facture {ligne.NumeroPaiement} en attente de règlement.";
        lblMontantDu.Text = $"Montant restant dû : {ligne.MontantRestantAffiche}";

        txtMontant.Text = ligne.MontantRestant.ToString("0", CultureInfo.InvariantCulture);

        // Pour une facture NORMALE deja emise, ConfirmerPaiement solde
        // toujours le montant exact de la facture : pas de saisie
        // partielle possible dans ce cas.
        if (!ligne.EstAcompteEnAttente)
        {
            txtMontant.IsReadOnly = true;
        }
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnValider_Click(object sender, RoutedEventArgs e)
    {
        string modePaiement = (cbMode.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Espèces";

        if (!decimal.TryParse(txtMontant.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal montant) || montant <= 0)
        {
            txtErreur.Text = "Montant invalide.";
            return;
        }
        if (montant > _ligne.MontantRestant)
        {
            txtErreur.Text = $"Le montant dépasse la somme restante due ({_ligne.MontantRestantAffiche}).";
            return;
        }

        try
        {
            if (_ligne.EstAcompteEnAttente)
            {
                var resultat = _paiementService.EncaisserAcompte(_ligne.NumeroRdv, montant, modePaiement);
                if (!resultat.Succes)
                {
                    txtErreur.Text = resultat.MessageErreur ?? "Le paiement n'a pas pu être enregistré.";
                    return;
                }
            }
            else
            {
                _paiementService.ConfirmerPaiement(_ligne.NumeroPaiement!, modePaiement);
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
