using System;
using Patients.Models;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using Patients.Services;
using Medecins.Services;
using System.Text.Json;

namespace Patients.Views.Medecin.ModifierMedecin;

public partial class ModifierMedecinView : Window
{
    // public ModifierMedecinView(Patients.Models.Medecin medecinAmodifier)
    private MedecinService _medecinService = new MedecinService();
    public ModifierMedecinView(Patients.Models.Medecin donneAModifier)
    {
        InitializeComponent();
        remplirChampParLesDonneesMedecin(donneAModifier);
    }

    private void BtnFermerModif_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    public void btnModifierMedecin_Click(object sender, RoutedEventArgs e)
    {
        // Vérif nom et prénom
        try
        {
            string nomPrenomRegex = @"^[a-zA-ZÀ-ÿ\s'-]{2,50}$";

            if (string.IsNullOrWhiteSpace(NomModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer un nom.";
                return;
            }
            if (!Regex.IsMatch(NomModif.Text, nomPrenomRegex))
            {
                txtMessageMedecin.Text = "Le nom contient des caractères invalides ou est trop court (2-50 caractères).";
                return;
            }

            if (string.IsNullOrWhiteSpace(PrenomModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer un prénom.";
                return;
            }
            if (!Regex.IsMatch(PrenomModif.Text, nomPrenomRegex))
            {
                txtMessageMedecin.Text = "Le prénom contient des caractères invalides ou est trop court (2-50 caractères).";
                return;
            }

            // vérif date de naissance
            if (DateDeNaissanceModif.SelectedDate == null)
            {
                txtMessageMedecin.Text = "Veuillez sélectionner une date de naissance.";
                return;
            }
            if (DateDeNaissanceModif.SelectedDate > DateTime.Now)
            {
                txtMessageMedecin.Text = "La date de naissance ne peut pas être dans le futur.";
                return;
            }
            if (DateDeNaissanceModif.SelectedDate < DateTime.Now.AddYears(-90))
            {
                txtMessageMedecin.Text = "La date de naissance ne peut pas être antérieure à 90 ans.";
                return;
            }
            if (DateDeNaissanceModif.SelectedDate > DateTime.Now.AddYears(-18))
            {
                txtMessageMedecin.Text = "Le médecin doit avoir au moins 18 ans.";
                return;
            }

            // vérif adresse avec un regex simple
            string adresseRegex = @"^[0-9a-zA-ZÀ-ÿ\s,.'-]{5,100}$";

            if (string.IsNullOrWhiteSpace(AdresseMedecinModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer une adresse.";
                return;
            }
            if (!Regex.IsMatch(AdresseMedecinModif.Text, adresseRegex))
            {
                txtMessageMedecin.Text = "L'adresse semble invalide ou est trop courte (min. 5 caractères).";
                return;
            }

            // vérif téléphone (jsp comment faire le regex alors j'ai fait le plus classique XD)
            string telephoneRegex = @"^\d{10}$";

            if (string.IsNullOrWhiteSpace(TelephoneMedecinModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer un numéro de téléphone.";
                return;
            }
            if (!Regex.IsMatch(TelephoneMedecinModif.Text, telephoneRegex))
            {
                txtMessageMedecin.Text = "Le numéro de téléphone doit contenir exactement 10 chiffres.";
                return;
            }

            // vérif email avec regex
            string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (string.IsNullOrWhiteSpace(EmailMedecinModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer une adresse e-mail.";
                return;
            }
            if (!Regex.IsMatch(EmailMedecinModif.Text, emailRegex))
            {
                txtMessageMedecin.Text = "Veuillez entrer une adresse e-mail valide.";
                return;
            }

            // tout le reste j'ai tassé ici...
            if (string.IsNullOrWhiteSpace(StatutMedecinModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer le statut du médecin.";
                return;
            }

            if (string.IsNullOrWhiteSpace(StatutMedecinModif.Text))
            {
                txtMessageMedecin.Text = "Veuillez entrer la fonction du médecin.";
                return;
            }

            if (!Decimal.TryParse(TauxHoraireMedecinModif.Text, out Decimal tauxHoraire) || tauxHoraire < 0)
            {
                // txtMessageMedecin.Text = "Veuillez entrer un taux horaire valide (nombre entier positif).";
                txtMessageMedecin.Text = $"Veuillez entrer un taux horaire valide (nombre entier positif).";
                return;
            }

            //regex de numero d'ordre de medecin 
            string ONMregex = @"^[0-9]{9}$";
            if (!Regex.IsMatch(numeroOrdreMedecinModif.Text, ONMregex))
            {
                txtMessageMedecin.Text = "Votre Numéro d'ordre de médecin est invalide";
                return;   
            }

            int code_fonc = _medecinService.RecupererCodeFonction(FonctionMedecinModif.Text);
            //On touche plus au matricule??
        
            // ici on appel le service enregistrerMedecin même si elle est encore vide 
            Patients.Models.Medecin donneModifierMedecin = new Patients.Models.Medecin
            {
                Id = IdMedecinModif.Text,
                Nom = NomModif.Text,
                Prenom = PrenomModif.Text,
                DateNaissance = DateDeNaissanceModif.SelectedDate.Value,
                //DateNaissance = DateOnly.Parse(DateDeNaissanceModif.Text),
                Genre = GenreModif.Text,
                Adresse = AdresseMedecinModif.Text,
                Telephone = TelephoneMedecinModif.Text,
                Email = EmailMedecinModif.Text,
                statut = StatutMedecinModif.Text,
                numero_ordre = numeroOrdreMedecinModif.Text,
                nom_fonction = FonctionMedecinModif.Text,
                code_fonction = code_fonc,
                taux_horaire = Decimal.Parse(TauxHoraireMedecinModif.Text)
            };

            bool estEnregistre = _medecinService.ModificationMedecin(donneModifierMedecin);

            if (estEnregistre)
            {
                txtMessageMedecin.Text = "Modification efféctué avec succès !";
                //je sais pas comment recharger la liste apres l'ajout et modification zut 
                this.Close();
                // .RechargerListeMedecin();
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

    public void remplirChampParLesDonneesMedecin(Patients.Models.Medecin donneMedecin)
    {
        IdMedecinModif.Text = donneMedecin.Id;
        NomModif.Text = donneMedecin.Nom;
        PrenomModif.Text = donneMedecin.Prenom;
        DateDeNaissanceModif.Text = donneMedecin.DateNaissance.ToString();
        GenreModif.Text = donneMedecin.Genre;
        AdresseMedecinModif.Text = donneMedecin.Adresse;
        TelephoneMedecinModif.Text = donneMedecin.Telephone;
        EmailMedecinModif.Text = donneMedecin.Email;
        StatutMedecinModif.Text = donneMedecin.statut;
        numeroOrdreMedecinModif.Text = donneMedecin.numero_ordre;
        FonctionMedecinModif.Text = donneMedecin.nom_fonction;
        TauxHoraireMedecinModif.Text = donneMedecin.taux_horaire.ToString();
    }
}
    
