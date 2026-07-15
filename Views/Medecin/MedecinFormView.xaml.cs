using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Patients.Services; 

namespace Patients.Views.Medecin;

public partial class MedecinFormView : UserControl
{
    private readonly MedecinService _medecinService = new MedecinService();

    public MedecinFormView()
    {
        InitializeComponent();
    }

    private void btnAjouterMedecin_Click(object sender, RoutedEventArgs e)
    {
        // Vérif nom et prénom
        string nomPrenomRegex = @"^[a-zA-ZÀ-ÿ\s'-]{2,50}$";

        if (string.IsNullOrWhiteSpace(txtNomMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer un nom.";
            return;
        }
        if (!Regex.IsMatch(txtNomMedecin.Text, nomPrenomRegex))
        {
            txtMessageMedecin.Text = "Le nom contient des caractères invalides ou est trop court (2-50 caractères).";
            return;
        }

        if (string.IsNullOrWhiteSpace(txtPrenomMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer un prénom.";
            return;
        }
        if (!Regex.IsMatch(txtPrenomMedecin.Text, nomPrenomRegex))
        {
            txtMessageMedecin.Text = "Le prénom contient des caractères invalides ou est trop court (2-50 caractères).";
            return;
        }

        // vérif date de naissance
        if (dpDateNaissanceMedecin.SelectedDate == null)
        {
            txtMessageMedecin.Text = "Veuillez sélectionner une date de naissance.";
            return;
        }
        if (dpDateNaissanceMedecin.SelectedDate > DateTime.Now)
        {
            txtMessageMedecin.Text = "La date de naissance ne peut pas être dans le futur.";
            return;
        }
        if (dpDateNaissanceMedecin.SelectedDate < DateTime.Now.AddYears(-90))
        {
            txtMessageMedecin.Text = "La date de naissance ne peut pas être antérieure à 90 ans.";
            return;
        }
        if (dpDateNaissanceMedecin.SelectedDate > DateTime.Now.AddYears(-18))
        {
            txtMessageMedecin.Text = "Le médecin doit avoir au moins 18 ans.";
            return;
        }

        // vérif adresse avec un regex simple
        string adresseRegex = @"^[0-9a-zA-ZÀ-ÿ\s,.'-]{5,100}$";

        if (string.IsNullOrWhiteSpace(txtAdresseMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer une adresse.";
            return;
        }
        if (!Regex.IsMatch(txtAdresseMedecin.Text, adresseRegex))
        {
            txtMessageMedecin.Text = "L'adresse semble invalide ou est trop courte (min. 5 caractères).";
            return;
        }

        // vérif téléphone (jsp comment faire le regex alors j'ai fait le plus classique XD)
        string telephoneRegex = @"^\d{10}$";

        if (string.IsNullOrWhiteSpace(txtTelephoneMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer un numéro de téléphone.";
            return;
        }
        if (!Regex.IsMatch(txtTelephoneMedecin.Text, telephoneRegex))
        {
            txtMessageMedecin.Text = "Le numéro de téléphone doit contenir exactement 10 chiffres.";
            return;
        }

        // vérif email avec regex
        string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        if (string.IsNullOrWhiteSpace(txtEmailMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer une adresse e-mail.";
            return;
        }
        if (!Regex.IsMatch(txtEmailMedecin.Text, emailRegex))
        {
            txtMessageMedecin.Text = "Veuillez entrer une adresse e-mail valide.";
            return;
        }

        // tout le reste j'ai tassé ici...
        if (string.IsNullOrWhiteSpace(txtStatutMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer le statut du médecin.";
            return;
        }

        if (string.IsNullOrWhiteSpace(txtFonctionMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer la fonction du médecin.";
            return;
        }

        if (!int.TryParse(txtTauxHoraireMedecin.Text, out int tauxHoraire) || tauxHoraire < 0)
        {
            txtMessageMedecin.Text = "Veuillez entrer un taux horaire valide (nombre entier positif).";
            return;
        }

        // ici on appel le service enregistrerMedecin même si elle est encore vide
        bool estEnregistre = _medecinService.EnregistrerMedecin(
            txtNomMedecin.Text,
            txtPrenomMedecin.Text,
            dpDateNaissanceMedecin.SelectedDate.Value,
            txtAdresseMedecin.Text,
            txtTelephoneMedecin.Text,
            txtEmailMedecin.Text,
            txtStatutMedecin.Text,
            txtFonctionMedecin.Text,
            tauxHoraire
        );

        if (estEnregistre)
        {
            txtMessageMedecin.Text = "Médecin ajouté avec succès !";
        }
        else
        {
            txtMessageMedecin.Text = "Une erreur est survenue lors de l'enregistrement dans le service.";
        }
    }
}