using System.Windows.Controls;
using Patients.Models;
using System.Windows;

namespace Patients.Views.Medecin.DetailEtDisponibilite;
public partial class DetailMedecin : UserControl
{
    public Patients.Models.Medecin _donneMedecin = new Patients.Models.Medecin();
    public Patients.Models.Disponibilite _disponibiliteDeMedecin = new Patients.Models.Disponibilite();
    //public Disponibilite medecinDisponibilite;
    public Patients.Services.DisponibiliteService _DisponibiliteService = new Patients.Services.DisponibiliteService();
    public DetailMedecin(Patients.Models.Medecin medecin_a_affiche)
    {
        InitializeComponent();
        _donneMedecin = medecin_a_affiche;
        DetailMedecin_a_Affichee(_donneMedecin);
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

    public void BtnValiderCreationDispo(Object sender, RoutedEventArgs e)
        {
            // 1. Liste pour stocker les codes (Tag) des options cochées
            List<String> tagCoches = new List<string>();

            // 3. Parcours de toutes les CheckBox dans le panneau 'pnlOptions'
            foreach (CheckBox chk in pnlOptions.Children.OfType<CheckBox>())
            {
                // On vérifie si la case est cochée
                if (chk.IsChecked == true)
                {
                    // Récupération de la valeur du Tag (code interne)
                    string code = chk.Tag?.ToString();
                    tagCoches.Add(code);
                }
            }

            // Si aucun block de temps n'a ete selectionner
            if (tagCoches.Count <= 0) {
                txtResultat.Text = "Aucune option n'a été cochée !";
                return;
            } 

            //si la date est deja passe
            if (!TXTDateDisponibilite.SelectedDate.HasValue)
            {
                txtResultat.Text = "Aucune Date n'a été choisie !";
                return;
            }
            
            // Exemple : combiner les résultats pour agles afficher
            //string messageCodes = string.Join(", ", tagCoches);
            //string messageNoms = string.Join(", ", tagCoches);
            //CreerDisponibilite(Disponibilite donneMedecin ,List<string> tabNumB

            DateTime datePicker = TXTDateDisponibilite.SelectedDate.Value;
            DateTime dateAujourdhui = DateTime.Now;

            if (datePicker < dateAujourdhui)
            {
                txtResultat.Text = "La date sélectionnée est dans le passé ! ";
                return;
            }

            _disponibiliteDeMedecin.id_medecin = _donneMedecin.Id;
            _disponibiliteDeMedecin.date_disponibilite = datePicker;

            bool estAjouter = _DisponibiliteService.CreerDisponibilite( _disponibiliteDeMedecin ,tagCoches);

            if (estAjouter)
            {
                 txtResultat.Text = $"Codes Ajouter ({tagCoches.Count})";
            } 
            else
            {
                txtResultat.Text = _DisponibiliteService.message;
            }
        }
}

//