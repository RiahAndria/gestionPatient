using System.Windows.Controls;
using System;
using System.Windows;

namespace Patients.Views.Medecin.DetailEtDisponibilite
{
    public partial class DashboardDispo : UserControl
    {
        // Date de début de la semaine affichée (par défaut, la semaine courante)
        private DateTime _dateDebutSemaine;
        public DashboardDispo()
        {
            InitializeComponent();
            _dateDebutSemaine = ObtenirLundiDeLaSemaine(DateTime.Now);
        }


        /// <summary>
        /// Helper pour trouver la date du Lundi d'une semaine donnée
        /// </summary>
        private DateTime ObtenirLundiDeLaSemaine(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// Navigation : Recule d'une semaine
        private void btnSemainePrecedente_Click(object sender, RoutedEventArgs e)
        {
            _dateDebutSemaine = _dateDebutSemaine.AddDays(-7);
            //ChargerDonneesExemple();
            
            // TODO (pour ton coéquipier) : 
            // Appeler ici la méthode du service/BDD pour recharger les disponibilités réelles de cette semaine
        }


        /// Navigation : Avance d'une semaine
        private void btnSemaineSuivante_Click(object sender, RoutedEventArgs e)
        {
            _dateDebutSemaine = _dateDebutSemaine.AddDays(7);
            //ChargerDonneesExemple();

            // TODO (pour ton coéquipier) : 
            // Appeler ici la méthode du service/BDD pour recharger les disponibilités réelles de cette semaine
        }
    }
}

//