using System.Windows;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Paiement;

// Apercu de facture, "papier" moderne, purement a l'ecran (pas
// d'impression ni d'export demande). Le bouton "Confirmer la
// facturation" passe EST_FACTURE a true cote base.
public partial class FactureWindow : Window
{
    private readonly PaiementService _paiementService = new();
    private readonly FactureDetail _detail;

    public FactureWindow(FactureDetail detail)
    {
        InitializeComponent();
        _detail = detail;

        lblNumero.Text = detail.NumeroPaiement;
        lblDate.Text = detail.DateReglement.ToString("dd/MM/yyyy à HH:mm");

        lblPatientNom.Text = detail.PatientNom;
        lblPatientMatricule.Text = $"Dossier n° {detail.PatientMatricule}";

        lblMedecinNom.Text = detail.MedecinNom;
        lblMedecinFonction.Text = detail.MedecinFonction;

        lblLibelle.Text = $"Consultation — {detail.TypePaiementAffiche}";
        lblRdv.Text = detail.NumeroRdv;

        lblMontant.Text = $"{detail.Montant:N0} Ar";
        lblMode.Text = $"Réglé par : {detail.ModePaiement}";
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        _paiementService.MarquerFacture(_detail.NumeroPaiement);
        Close();
    }
}
