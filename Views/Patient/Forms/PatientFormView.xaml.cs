using System;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Patient.Forms;

public partial class PatientFormView : UserControl
{
    private readonly PatientService _patientService = new PatientService();

    public PatientFormView()
    {
        InitializeComponent();
    }

    private void btnAjouter_Click(object sender, RoutedEventArgs e)
    {
        // Validation des champs obligatoires
        if (string.IsNullOrWhiteSpace(txtNom.Text) || string.IsNullOrWhiteSpace(txtPrenom.Text) || dpDateNaissance.SelectedDate == null)
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = "Veuillez remplir les champs obligatoires (Nom, Prénom, Date de naissance).";
            return;
        }

        string genreSelectionne = (cbGenre.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Autre";
        bool aUneAssurance = !string.IsNullOrWhiteSpace(txtNumAssurancePatient.Text);

        // Génération automatique du matricule unique (trop fier de cette foncion XD)
        string nouveauMatricule = MatriculeHelper.GenererMatricule(genreSelectionne, aUneAssurance);

        // Création du modèle Patient
        Models.Patient nouveauPatient = new Models.Patient
        {
            Id = Guid.NewGuid().ToString(), // Identifiant unique
            Nom = txtNom.Text,
            Prenom = txtPrenom.Text,
            DateNaissance = dpDateNaissance.SelectedDate.Value,
            Genre = genreSelectionne,
            Adresse = txtAdressePatient.Text,
            Telephone = txtTelephonePatient.Text,
            Email = txtEmailPatient.Text,
            NumeroDossier = nouveauMatricule,
            NumeroAssurance = txtNumAssurancePatient.Text
        };

        try
        {
            _patientService.AjouterPatient(nouveauPatient);

            // Rafraîchir le tableau via MainWindow sans exposer directement le contrôle interne
            if (Window.GetWindow(this) is MainWindow principal)
            {
                principal.RefreshPatientList();
            }

            ViderFormulaire();

            txtMessage.Foreground = System.Windows.Media.Brushes.Green;
            txtMessage.Text = $"Patient ajouté avec succès en base de données ! Matricule : {nouveauMatricule}";
        }
        catch (Exception ex)
        {
            txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            txtMessage.Text = $"Erreur BDD : {ex.Message}";
        }
    }

    private void ViderFormulaire()
    {
        txtNom.Clear();
        txtPrenom.Clear();
        dpDateNaissance.SelectedDate = null;
        txtAdressePatient.Clear();
        txtTelephonePatient.Clear();
        txtEmailPatient.Clear();
        txtNumAssurancePatient.Clear();
    }
}