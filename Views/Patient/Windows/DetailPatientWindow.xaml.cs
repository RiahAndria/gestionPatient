using System;
using System.Windows;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Patient;

public partial class DetailPatientWindow : Window
{
    private readonly PatientService _patientService = new();
    private Models.Patient? _patient;

    public DetailPatientWindow(Models.Patient? patient, Dossier? dossier = null)
    {
        InitializeComponent();
        _patient = patient;

        AfficherInfoPatient();
        AfficherInfoDossier(dossier);
    }

    private void AfficherInfoPatient()
    {
        if (_patient is null) return;

        lblIdentite.Text = $"{_patient.Nom?.ToUpper()} {_patient.Prenom}";
        lblMatricule.Text = $"DOSSIER N° {_patient.NumeroDossier}";
        lblContact.Text = $"Tél: {_patient.Telephone}\nEmail: {_patient.Email}\nAdresse: {_patient.Adresse}";
        lblAssurance.Text = string.IsNullOrWhiteSpace(_patient.NumeroAssurance)
            ? "Assurance: Non renseignée"
            : $"N° Assurance: {_patient.NumeroAssurance}";
    }

    private void AfficherInfoDossier(Dossier? dossier)
    {
        if (dossier != null)
        {
            lblPoids.Text = $"{dossier.Poids} kg";
            lblTaille.Text = $"{dossier.Taille} cm";
            lblGroupe.Text = string.IsNullOrWhiteSpace(dossier.GroupeSanguin) ? "-" : dossier.GroupeSanguin;
            txtTraitements.Text = string.IsNullOrWhiteSpace(dossier.Traitement)
                ? "Pas de traitement actif."
                : dossier.Traitement;
            txtAllergies.Text = string.IsNullOrWhiteSpace(dossier.Allergies)
                ? "Aucune allergie connue."
                : dossier.Allergies;
            txtAntecedents.Text = string.IsNullOrWhiteSpace(dossier.Antecedents)
                ? "Aucun antécédent répertorié."
                : dossier.Antecedents;
        }
        else
        {
            lblPoids.Text = "-- kg";
            lblTaille.Text = "-- cm";
            lblGroupe.Text = "--";
            txtTraitements.Text = "Aucun dossier médical enregistré.";
            txtAllergies.Text = "Aucune donnée.";
            txtAntecedents.Text = "Aucune donnée.";
        }
    }

    private void BtnOuvrirModification_Click(object sender, RoutedEventArgs e)
    {
        if (_patient is null) return;

        var editWindow = new Windows.EditPatientWindow(_patient) 
        { 
            Owner = this
        };

        if (editWindow.ShowDialog() == true)
        {
            AfficherInfoPatient();

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.RefreshPatientList();
            }
        }
    }

    private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
    {
        if (_patient is null) return;

        var confirmation = MessageBox.Show(
            $"Voulez-vous vraiment supprimer le patient {_patient.Nom} {_patient.Prenom} ?",
            "Confirmation de suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            _patientService.SupprimerPatient(_patient.Id);
            
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.RefreshPatientList();
            }
            
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}