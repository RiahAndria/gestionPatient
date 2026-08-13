using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Patients.Services;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape6PaiementView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly Action _allerPrecedent;
    private readonly PaiementService _paiementService = new();
    private readonly RappelService _rappelService = new();

    private bool _initialisationEnCours = true;

    public Etape6PaiementView(AssistantRendezVousState etat, Action allerSuivant, Action allerPrecedent)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _allerPrecedent = allerPrecedent;

        TxtMontantTotal.Text = $"Montant à régler : {_etat.MontantTotal:N0} Ar";

        AppliquerModeEntier();
        _initialisationEnCours = false;
    }

    private decimal MontantMinimumAvance =>
        Math.Round(_etat.MontantTotal * PaiementService.POURCENTAGE_ACOMPTE_MINIMUM, 0, MidpointRounding.AwayFromZero);

    private void TypeReglement_Changed(object sender, RoutedEventArgs e)
    {
        if (RadioEntier == null || RadioAvance == null) return; // appele pendant InitializeComponent

        if (RadioEntier.IsChecked == true) AppliquerModeEntier();
        else AppliquerModeAvance();
    }

    private void AppliquerModeEntier()
    {
        TxtInfoAvance.Visibility = Visibility.Collapsed;
        TxtResteAPayer.Visibility = Visibility.Collapsed;
        TxtMontant.IsReadOnly = true;
        TxtMontant.Text = _etat.MontantTotal.ToString("0", CultureInfo.InvariantCulture);
        TxtErreur.Text = "";
    }

    private void AppliquerModeAvance()
    {
        TxtInfoAvance.Visibility = Visibility.Visible;
        TxtInfoAvance.Text = $"Montant minimum d'avance : {MontantMinimumAvance:N0} Ar (60 % du tarif).";
        TxtMontant.IsReadOnly = false;
        TxtMontant.Text = MontantMinimumAvance.ToString("0", CultureInfo.InvariantCulture);
        TxtResteAPayer.Visibility = Visibility.Visible;
        TxtErreur.Text = "";
        MettreAJourResteAPayer();
    }

    private void TxtMontant_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_initialisationEnCours) return;
        MettreAJourResteAPayer();
    }

    private void MettreAJourResteAPayer()
    {
        if (RadioAvance.IsChecked != true) return;

        if (decimal.TryParse(TxtMontant.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal montant))
        {
            decimal reste = Math.Max(0, _etat.MontantTotal - montant);
            TxtResteAPayer.Text = $"Somme restante due (avant/pendant la consultation) : {reste:N0} Ar";
        }
        else
        {
            TxtResteAPayer.Text = "";
        }
    }

    private void BtnPrecedent_Click(object sender, RoutedEventArgs e) => _allerPrecedent();

    private void BtnValider_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_etat.NumeroRdvCree))
        {
            TxtErreur.Text = "Le rendez-vous n'a pas encore été créé, reviens à l'étape précédente.";
            return;
        }

        // Regex/controle de saisie : uniquement des chiffres (et
        // eventuellement une decimale), jamais de texte libre, comme
        // demande par la regle metier de l'etape 6.
        string texteMontant = TxtMontant.Text?.Trim() ?? "";
        if (!System.Text.RegularExpressions.Regex.IsMatch(texteMontant, @"^\d+([.,]\d{1,2})?$"))
        {
            TxtErreur.Text = "Montant invalide : utilise uniquement des chiffres.";
            return;
        }

        decimal montant = decimal.Parse(texteMontant.Replace(',', '.'), CultureInfo.InvariantCulture);
        bool estAvance = RadioAvance.IsChecked == true;

        var (estValide, erreur) = _paiementService.ValiderMontantSaisi(_etat.MontantTotal, montant, estAvance);
        if (!estValide)
        {
            TxtErreur.Text = erreur;
            return;
        }

        string modePaiement = (CbModePaiement.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Espèces";

        try
        {
            BtnValider.IsEnabled = false;
            var resultat = _paiementService.EncaisserAcompte(_etat.NumeroRdvCree, montant, modePaiement);

            if (!resultat.Succes)
            {
                TxtErreur.Text = resultat.MessageErreur ?? "Le paiement n'a pas pu être enregistré.";
                BtnValider.IsEnabled = true;
                return;
            }

            _etat.PaiementEffectue = true;
            _etat.PaiementComplet = resultat.PaiementComplet;
            _etat.MontantVerse = montant;
            _etat.MontantRestant = resultat.MontantRestant;
            _etat.ModePaiementChoisi = modePaiement;

            // Declenche le module de notification/alerte deja existant
            // (visible dans la sidebar "Notifications").
            string messageNotif = resultat.PaiementComplet
                ? $"Rendez-vous {_etat.NumeroRdvCree} confirmé et réglé intégralement ({modePaiement})."
                : $"Rendez-vous {_etat.NumeroRdvCree} confirmé avec une avance de {montant:N0} Ar ({modePaiement}). Reste dû : {resultat.MontantRestant:N0} Ar.";
            try { _rappelService.CreerNotification(_etat.NumeroRdvCree, messageNotif); }
            catch { /* la notification est un plus, on ne bloque pas la confirmation du RDV si elle echoue */ }

            _allerSuivant();
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Erreur inattendue : {ex.Message}";
            BtnValider.IsEnabled = true;
        }
    }
}
