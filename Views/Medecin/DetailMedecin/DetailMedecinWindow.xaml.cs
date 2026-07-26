using System;
using System.Windows;
using System.Windows.Controls;
// using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using Medecins.Services;
using Patients.Models;

namespace Patient.Views.Medecin.DetailMedecin
{
    public partial class DetailMedecinWindow : Window
    {

        public DetailMedecinWindow(Patients.Models.Medecin medecin_a_affiche)
        {
            InitializeComponent();
            DetailMedecin_a_Affichee(medecin_a_affiche);
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void DetailMedecin_a_Affichee(Patients.Models.Medecin donnee_medecin)
        {
            
            TXTMatricule.Text = donnee_medecin.Id ;
            TXTIdentite.Text = donnee_medecin.Nom + " " + donnee_medecin.Prenom;
            TXTDateNaissance.Text = $"Née le {donnee_medecin.DateNaissance}";
            TXTAdress.Text = donnee_medecin.Adresse;
            TXTContact.Text = $"Tel- {donnee_medecin.Telephone} et email- {donnee_medecin.Email}";
            TXTOrdreMedecin.Text = donnee_medecin.numero_ordre;
            TXTInformationProfessionnel.Text = $"{donnee_medecin.nom_fonction} - {donnee_medecin.statut}";
            TXTTauxMhoraire.Text = $"{donnee_medecin.taux_horaire}";
        }
    }
}

