using System;
using System.Windows;
using System.Windows.Controls;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape7ConfirmationView : UserControl
{
    private readonly Action _terminer;

    public Etape7ConfirmationView(AssistantRendezVousState etat, Action terminer)
    {
        InitializeComponent();
        _terminer = terminer;

        TxtSousTitre.Text = "Le rendez-vous a bien été enregistré.";
        TxtNumero.Text = $"N° rendez-vous : {etat.NumeroRdvCree}";
        TxtPatient.Text = $"Patient : {etat.Patient?.Nom} {etat.Patient?.Prenom}";
        TxtMedecin.Text = $"Médecin : {etat.Medecin?.NomComplet} — {etat.Service?.NomService}";
        TxtDateHeure.Text = $"Date et heure : {etat.DateHeureRdv:dddd dd/MM/yyyy} — {etat.Creneau?.Libelle}";

        TxtStatutPaiement.Text = etat.PaiementComplet
            ? $"Paiement effectué en totalité ({etat.MontantVerse:N0} Ar, {etat.ModePaiementChoisi})."
            : $"Avance de {etat.MontantVerse:N0} Ar réglée ({etat.ModePaiementChoisi}). Reste dû : {etat.MontantRestant:N0} Ar.";
    }

    private void BtnRetour_Click(object sender, RoutedEventArgs e) => _terminer();
}
