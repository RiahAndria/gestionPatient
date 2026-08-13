using System;
using System.Windows;
using System.Windows.Controls;
using Patients.Helpers;
using Patients.Services;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape5RecapitulatifView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly Action _allerPrecedent;
    private readonly RendezVousService _rendezVousService = new();

    public Etape5RecapitulatifView(AssistantRendezVousState etat, Action allerSuivant, Action allerPrecedent)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _allerPrecedent = allerPrecedent;

        TxtPatient.Text = $"Patient : {_etat.Patient?.Nom} {_etat.Patient?.Prenom} ({_etat.Patient?.NumeroDossier})";
        TxtService.Text = $"Service : {_etat.Service?.NomService}";
        TxtMedecin.Text = $"Médecin : {_etat.Medecin?.NomComplet} — {_etat.Medecin?.Fonction}";
        TxtDateHeure.Text = $"Date et heure : {_etat.DateHeureRdv:dddd dd/MM/yyyy} — {_etat.Creneau?.Libelle}";

        TxtMotif.Text = _etat.Motif;
    }

    private void BtnPrecedent_Click(object sender, RoutedEventArgs e) => _allerPrecedent();

    private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        if (_etat.Patient is null || _etat.Service is null || _etat.Medecin is null || _etat.Creneau is null)
        {
            TxtErreur.Text = "Des informations sont manquantes, reviens aux étapes précédentes.";
            return;
        }

        _etat.Motif = TxtMotif.Text?.Trim() ?? "";

        try
        {
            BtnConfirmer.IsEnabled = false;
            string numero = NumeroRdvHelper.GenererNumero();

            _rendezVousService.AjouterRendezVous(new Patients.Models.RendezVous
            {
                NumRendezVous = numero,
                PatientID = _etat.Patient.Id,
                MedecinID = _etat.Medecin.Id,
                DateHeure = _etat.DateHeureRdv,
                Motif = _etat.Motif
            });

            // Tarif suggere = taux horaire du medecin (meme regle que
            // PaiementService.CalculerMontantSuggereParRdv), utilise a
            // l'etape 6 comme base du paiement.
            _etat.NumeroRdvCree = numero;
            _etat.MontantTotal = _etat.Medecin.TauxHoraire;

            _allerSuivant();
        }
        catch (InvalidOperationException ex)
        {
            // Erreur metier attendue (ex : creneau pris entre-temps par
            // un autre secretariat) : message clair, on reste sur cette etape.
            TxtErreur.Text = ex.Message;
            BtnConfirmer.IsEnabled = true;
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Erreur inattendue : {ex.Message}";
            BtnConfirmer.IsEnabled = true;
        }
    }
}
