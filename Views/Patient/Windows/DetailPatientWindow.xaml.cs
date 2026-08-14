using System;
using System.Windows;
using Patients.Helpers;
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

        if (_patient is not null && dossier is null)
        {
            dossier = _patientService.ObtenirDossierMedical(_patient.NumeroDossier);
        }

        AfficherInfoPatient();
        AfficherInfoDossier(dossier);
    }

    private void AfficherInfoPatient()
    {
        if (_patient is null) return;

        lblIdentite.Text = $"{PatientHelper.ObtenirTitrePatient(_patient.Genre, _patient.DateNaissance)} {_patient.Nom?.ToUpper()} {_patient.Prenom}";
        lblMatricule.Text = $"DOSSIER N° {_patient.NumeroDossier}";
        lblContact.Text = $"Tél: {_patient.Telephone}\nEmail: {_patient.Email}\nAdresse: {_patient.Adresse}";
        lblDateNaissance.Text = $"Date de naissance : {_patient.DateNaissance:dd/MM/yyyy}";
        lblAgeGenre.Text = PatientHelper.ObtenirDetailPatient(_patient.Genre, _patient.DateNaissance);
        lblAssurance.Text = string.IsNullOrWhiteSpace(_patient.NumeroAssurance)
            ? "Assurance: Non renseignée"
            : $"N° Assurance: {_patient.NumeroAssurance}";
    }

    private static bool EstDossierMedicalVide(Dossier? dossier)
    {
        if (dossier is null)
        {
            return true;
        }

        bool infosPhysiquesVides =
            dossier.Poids <= 0m &&
            dossier.Taille <= 0m &&
            (string.IsNullOrWhiteSpace(dossier.GroupeSanguin) || dossier.GroupeSanguin.Equals("N/A", StringComparison.OrdinalIgnoreCase));

        return infosPhysiquesVides &&
               string.IsNullOrWhiteSpace(dossier.Allergies) &&
               string.IsNullOrWhiteSpace(dossier.Antecedents) &&
               string.IsNullOrWhiteSpace(dossier.Traitement);
    }

    private void AfficherInfoDossier(Dossier? dossier)
    {
        if (dossier != null && !EstDossierMedicalVide(dossier))
        {
            lblPoids.Text = dossier.Poids > 0m ? $"{dossier.Poids} kg" : "-- kg";
            lblTaille.Text = dossier.Taille > 0m ? $"{dossier.Taille} cm" : "-- cm";
            lblGroupe.Text = string.IsNullOrWhiteSpace(dossier.GroupeSanguin) || dossier.GroupeSanguin.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                ? "--"
                : dossier.GroupeSanguin;

            if (string.IsNullOrWhiteSpace(dossier.NumeroAssurance) && _patient != null)
            {
                dossier.NumeroAssurance = _patient.NumeroAssurance;
            }

            if (!string.IsNullOrWhiteSpace(dossier.NumeroAssurance))
            {
                lblAssurance.Text = $"N° Assurance: {dossier.NumeroAssurance}";
            }

            txtTraitements.Text = string.IsNullOrWhiteSpace(dossier.Traitement)
                ? "Pas de traitement actif."
                : dossier.Traitement;
            txtAllergies.Text = string.IsNullOrWhiteSpace(dossier.Allergies)
                ? "Aucune allergie connue."
                : dossier.Allergies;
            txtAntecedents.Text = string.IsNullOrWhiteSpace(dossier.Antecedents)
                ? "Aucun antécédent répertorié."
                : dossier.Antecedents;
            return;
        }

        lblPoids.Text = "-- kg";
        lblTaille.Text = "-- cm";
        lblGroupe.Text = "--";
        txtTraitements.Text = "Aucun dossier médical enregistré.";
        txtAllergies.Text = "Aucune donnée.";
        txtAntecedents.Text = "Aucune donnée.";
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

    private void BtnHistorique_Click(object sender, RoutedEventArgs e)
    {
        if (_patient is null)
        {
            return;
        }

        try
        {
            var consultations = _patientService.ObtenirConsultationsParPatient(_patient.Id);
            var historiqueWindow = new Windows.HistoriqueConsultationsWindow(_patient, consultations)
            {
                Owner = this
            };

            historiqueWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de l'historique : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}