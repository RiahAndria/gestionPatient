using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Patients.Helpers;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Patient.Windows;

public partial class EditPatientWindow : Window
{
    private readonly PatientService _patientService = new PatientService();
    private readonly Models.Patient _patientActuel;

    public EditPatientWindow(Models.Patient patient)
    {
        InitializeComponent();
        _patientActuel = patient ?? throw new ArgumentNullException(nameof(patient));

        ChargerDonneesPatient();
    }

    private void ChargerDonneesPatient()
    {
        txtMatriculeDisplay.Text = _patientActuel.NumeroDossier;
        txtNom.Text = _patientActuel.Nom;
        txtPrenom.Text = _patientActuel.Prenom;
        dpDateNaissance.SelectedDate = _patientActuel.DateNaissance;
        txtAdressePatient.Text = _patientActuel.Adresse;
        txtTelephonePatient.Text = _patientActuel.Telephone;
        txtEmailPatient.Text = _patientActuel.Email;
        txtNumAssurancePatient.Text = _patientActuel.NumeroAssurance;

        // Sélection du genre dans le ComboBox
        foreach (ComboBoxItem item in cbGenre.Items)
        {
            if (item.Content.ToString() == _patientActuel.Genre)
            {
                cbGenre.SelectedItem = item;
                break;
            }
        }
    }

    private void btnEnregistrer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNom.Text) || string.IsNullOrWhiteSpace(txtPrenom.Text) || dpDateNaissance.SelectedDate == null)
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "Veuillez remplir les champs obligatoires (*).";
            return;
        }

        if (!PatientHelper.EstNomValide(txtNom.Text) || !PatientHelper.EstNomValide(txtPrenom.Text))
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "Le nom et le prénom ne doivent contenir que des lettres, espaces, apostrophes ou tirets.";
            return;
        }

        if (!PatientHelper.EstDateNaissanceValide(dpDateNaissance.SelectedDate))
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "La date de naissance doit être valide, non future et inférieure à 100 ans.";
            return;
        }

        if (!PatientHelper.EstEmailValide(txtEmailPatient.Text))
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "L'adresse e-mail doit respecter le format texte@texte.texte.";
            return;
        }

        if (!PatientHelper.EstTelephoneValide(txtTelephonePatient.Text))
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "Le numéro de téléphone doit contenir exactement 10 chiffres.";
            return;
        }

        string genreSelectionne = (cbGenre.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Autre";

        // Mise à jour de l'objet
        _patientActuel.Nom = txtNom.Text.Trim();
        _patientActuel.Prenom = txtPrenom.Text.Trim();
        _patientActuel.DateNaissance = dpDateNaissance.SelectedDate.Value;
        _patientActuel.Genre = genreSelectionne;
        _patientActuel.Adresse = txtAdressePatient.Text.Trim();
        _patientActuel.Telephone = txtTelephonePatient.Text.Trim();
        _patientActuel.Email = txtEmailPatient.Text.Trim();
        _patientActuel.NumeroAssurance = txtNumAssurancePatient.Text.Trim();

        try
        {
            _patientService.ModifierPatient(_patientActuel);

            this.DialogResult = true; // Indique que la modification a réussi
            this.Close();
        }
        catch (Exception ex)
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = $"Erreur lors de la sauvegarde : {ex.Message}";
        }
    }

    private void btnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}