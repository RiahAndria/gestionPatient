using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Medecin.DetailEtDisponibilite
{
    public partial class DashboardDispo : UserControl
    {
        private DateTime _dateDebutSemaine;
        private List<AgendaJournee> _agendaSemaine;
        private DisponibiliteService _disponibiliteService = new DisponibiliteService();
        private Patients.Models.Medecin _donneMedecin = new Patients.Models.Medecin();

        public DashboardDispo(Patients.Models.Medecin medecin)
        {   
            InitializeComponent();

            //recuperation de la deonne du medecin
            _donneMedecin = medecin;

            //remplissage de l'header contenant un peu dde donne du medecin
            remplissageDonneAffiche();

            //baleur par defaut
            _dateDebutSemaine = ObtenirLundiDeLaSemaine(DateTime.Now);

            // On s'abonne avec de la gestion d'erreur
            this.Loaded += async (sender, e) =>
            {
                try
                {
                    await SemaineCourant(_dateDebutSemaine);
                    //MessageBox.Show("erreur");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur au chargement : {ex.Message}\n\nDétails : {ex.StackTrace}");
                }
            };
        }

        private DateTime ObtenirLundiDeLaSemaine(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private async Task SemaineCourant(DateTime LundiDeLaSemaine)
        {  

            // Récupération des données du service
            _agendaSemaine = await _disponibiliteService.ObtenirAgendaUneSemaine(_donneMedecin.Id , LundiDeLaSemaine);
            
            if (_agendaSemaine == null)
            {
                MessageBox.Show("Le service a renvoyé une liste NULL.");
                return;
            }

            foreach (AgendaJournee agendaJournee in _agendaSemaine)
            {
                
                if (agendaJournee?.Creneaux15Min == null) continue;

                foreach (Temps tmp in agendaJournee.Creneaux15Min)
                {
                    ColorerCreneau( tmp.date_disponibilite , tmp.heure_debut, tmp.heure_fin , tmp.est_reserve);
                }
            }
        }


        private void ColorerCreneau(DateTime date, DateTime dateHeureDebut, DateTime dateHeureFin, bool estReserver)
        {
            // Formatage propre du jour (lun, mar, mer...) sans le point
            string nomJour = date.ToString("ddd", new CultureInfo("fr-FR"))
                                           .Replace(".", "")
                                           .Substring(0, 3)
                                           .ToLower();

            string heure_debut = dateHeureDebut.ToString("HHmm");
            string heure_fin = dateHeureFin.ToString("HHmm");

            // Reconstruction du x:Name WPF (ex: mer_0800_0815)
            string nomBorder = $"{nomJour}_{heure_debut}_{heure_fin}";

            // Recherche robuste : d'abord via FindName, puis via l'arbre logique si FindName échoue
            Border? borderTrouve = (this.FindName(nomBorder) as Border) 
                                ?? (LogicalTreeHelper.FindLogicalNode(this, nomBorder) as Border);

            if (borderTrouve != null)
            {
                
                if (estReserver)
                {
                    // Rouge pour occupé
                    borderTrouve.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    borderTrouve.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }
                else
                {
                    // Vert pour libre
                    borderTrouve.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    borderTrouve.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }
            }
            else
            {
                // Cet affichage permettra de voir EXACTEMENT quel x:Name manque dans votre XAML
                MessageBox.Show($"Introuvable dans le XAML : '{nomBorder}'");
            }
        }

        private void remplissageDonneAffiche()
        {
            txtNomMedecinHeader.Text = _donneMedecin.Nom + " " + _donneMedecin.Prenom;
            txtSpecialiteHeader.Text = _donneMedecin.nom_fonction;
            txtContactHeader.Text = _donneMedecin.Telephone;
        }

        /// <summary>
        /// Navigation : Recule d'une semaine
        /// </summary>
        public async void btnSemainePrecedente_Click(object sender, RoutedEventArgs e)
        {
            _dateDebutSemaine = _dateDebutSemaine.AddDays(-7);
            await SemaineCourant(_dateDebutSemaine);
        }

        /// <summary>
        /// Navigation : Avance d'une semaine
        /// </summary>
        public async void btnSemaineSuivante_Click(object sender, RoutedEventArgs e)
        {
            _dateDebutSemaine = _dateDebutSemaine.AddDays(7);
            await SemaineCourant(_dateDebutSemaine);
        }
    }
}