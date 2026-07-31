using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Medecins.Services;
using Patients.Models;
using Patients.Helpers;
using Patients.Views.Medecin.ListeMedecin;


namespace Patients.Views.Medecin;

public partial class MedecinFormView : UserControl
{
    private readonly MedecinService _medecinService = new MedecinService();

    public MedecinFormView()
    {
        InitializeComponent();
    }


    /*Tache reste à faire : realiser les filitre de regex 
        (ONM)
        creation de fonction qui genere de matricule
    */
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
            // txtMessageMedecin.Text = "Veuillez entrer un taux horaire valide (nombre entier positif).";
            txtMessageMedecin.Text = $"Veuillez entrer un taux horaire valide (nombre entier positif).";
            return;
        }

        //regex de numero d'ordre de medecin 
        string ONMregex = @"^[0-9]{9}$";
        if (!Regex.IsMatch(numeroOrdreMedecin.Text, ONMregex))
        {
            txtMessageMedecin.Text = "Votre Numéro d'ordre de médecin est invalide";
            return;   
        }

        //generer matricule
        string matricule = MatriculeHelperMedecin.GenererMatricule(cbGenreMedecin.Text , txtFonctionMedecin.Text);
        
        //recuperer le code_focntion
        // MedecinService serviceMed = new MedecinService();
        int code_fonc = _medecinService.RecupererCodeFonction(txtFonctionMedecin.Text);

        // ici on appel le service enregistrerMedecin même si elle est encore vide 
        try
        {
            Patients.Models.Medecin nouveauMedecin = new Patients.Models.Medecin
            {
                Id = matricule,
                Nom = txtNomMedecin.Text,
                Prenom = txtPrenomMedecin.Text,
                DateNaissance = dpDateNaissanceMedecin.SelectedDate.Value,
                Genre = cbGenreMedecin.Text,
                Adresse = txtAdresseMedecin.Text,
                Telephone = txtTelephoneMedecin.Text,
                Email = txtEmailMedecin.Text,
                statut = txtStatutMedecin.Text,
                numero_ordre = numeroOrdreMedecin.Text,
                code_fonction = code_fonc,
                nom_fonction = txtFonctionMedecin.Text,
                taux_horaire = tauxHoraire,
            };

            bool estEnregistre = _medecinService.AjouterMedecin(nouveauMedecin);

            if (estEnregistre)
            {
                MaPage MedecinFormV = new MaPage();
                MedecinFormV.RechargerListeMedecin();
                txtMessageMedecin.Text = "Médecin ajouté avec succès !";
                // _medecinService.ObtenirTousLesMedecin();
            }
            else
            {
                txtMessageMedecin.Text = "Une erreur est survenue lors de l'enregistrement dans le service.";
            }
        } 
        catch
        {
            txtMessageMedecin.Text = "Erreur de connexion.";   
        }
    }
}