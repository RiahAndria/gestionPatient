using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Medecins.Services;
using Patients.Models;
using Patients.Helpers;
using Patients.Services;
using Patients.Views.Medecin.ListeMedecin;
using System.Windows.Documents;
using Microsoft.VisualBasic;
using Patients.Views.Medecin.AjoutFonction;


namespace Patients.Views.Medecin;

public partial class MedecinFormView : UserControl
{
    private readonly MedecinService _medecinService = new MedecinService();
    private readonly FonctionService _fonctionService = new FonctionService();
    public FonctionService service = new FonctionService();

    public int TXTCodeFonction { get; set; }
    public List<Fonction> ListeDesFonctions { get; set; } = new List<Fonction>();

    public MedecinFormView()
    {
        InitializeComponent();
        //Définir le DataContext sur la vue elle-même pour autoriser le Binding
        this.DataContext = this;
        // 1. Appeler le service pour récupérer la liste
        RechargerListeFonction();

        
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
        string telRegex = "^(032|033|034|037|038)[0-9]{7}";

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

        if (!Regex.IsMatch(txtTelephoneMedecin.Text, telRegex))
        {
            txtMessageMedecin.Text = "Le numéro de téléphone doit commencer par : 032/033/034/037/038";
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
        string matricule = MatriculeHelperMedecin.GenererMatricule(cbGenreMedecin.Text , TXTCodeFonction);
        
        //recuperer le code_focntion
        int code_fonc = TXTCodeFonction;

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
                //cette partie aussi est modifier
                nom_fonction = "vary be menaka ny laoka",
                taux_horaire = tauxHoraire,
            };
            
            bool estEnregistre = _medecinService.AjouterMedecin(nouveauMedecin);

            if (estEnregistre)
            {
                MaPage MedecinFormV = new MaPage();
                MedecinFormV.RechargerListeMedecin();
                ViderFormulaire();
                txtMessageMedecin.Text = "Médecin ajouté avec succès !";
                // _medecinService.ObtenirTousLesMedecin();
            }
            else
            {
                txtMessageMedecin.Text = _medecinService.message;
            }
        } 
        catch (Exception ex)
        {
            // Affiche le vrai message d'erreur pour faciliter le débogage
                txtMessageMedecin.Text = $"Erreur : {ex.Message}";
                Console.WriteLine($"[ERREUR DÉTAILLÉE] {ex}");  
        }
    }

    private void ViderFormulaire()
    {
        txtNomMedecin.Clear();
        txtPrenomMedecin.Clear();
        dpDateNaissanceMedecin.SelectedDate = null;
        txtAdresseMedecin.Clear();
        txtTelephoneMedecin.Clear();
        txtEmailMedecin.Clear();
        txtStatutMedecin.Clear();
        numeroOrdreMedecin.Clear();
    }

    public void AjoutFOnction(object sender, RoutedEventArgs e)
    {
        AjoutFonctionView fenetre = new AjoutFonctionView();
        fenetre.Owner = Window.GetWindow(this);
        bool? estFermer = fenetre.ShowDialog();
        
        if (estFermer == true)
        {
            RechargerListeFonction();
        }
    }
    
    public void RechargerListeFonction()
    {
        //ListeDesFonctions.ItemsSource = null;
        var ListeFonctionBD = service.recupererLeListeDesFonctions();
        ListeDesFonctions.Clear();
        foreach (Fonction chaqueFonction in ListeFonctionBD)
        {
            //MessageBox.Show(chaqueFonction.nom_fonction);
            ListeDesFonctions.Add(chaqueFonction);
        }
    }
}