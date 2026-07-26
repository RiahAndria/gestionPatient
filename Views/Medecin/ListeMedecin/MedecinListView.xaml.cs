using System.Windows.Controls;
using System.Windows;
using Patients.Models;
using Medecins.Services;
using System.Security.Cryptography.X509Certificates;
using System.Security.AccessControl;
using Patient.Views.Medecin.DetailMedecin;
using Patients.Views.Medecin.ModifierMedecin;
using System.IO.Compression;

namespace Patients.Views.Medecin.ListeMedecin
{

	public partial class MaPage : UserControl
	{   
        public MedecinService instanceServiceMedecin = new MedecinService();
		public MaPage()
		{
            InitializeComponent();
            this.Loaded += (sender, e) => RechargerListeMedecin();
		}

        public void RechargerListeMedecin()
        {
            dgSimple.ItemsSource = instanceServiceMedecin.ObtenirTousLesMedecin();
        }    

        public void BtnOuvrirFenetre_Click(Object sender, RoutedEventArgs e)
        {
            //recuperation du valeur de Tag
            DataGrid grid = sender as DataGrid;
            
            if (grid?.SelectedItem is Patients.Models.Medecin medecinSelectionne)
            {  
                //string valeurTag = medecinSelectionne.Id.ToString();

                DetailMedecinWindow fenetre = new DetailMedecinWindow(medecinSelectionne);
                fenetre.Owner = Window.GetWindow(this);
                fenetre.ShowDialog();
            }
        }

        public void BtnOuvrirFenetre_Modification(Object sender, RoutedEventArgs e)
        {
            //recuperation du valeur de Tag 


            //appel de foncion qui creer la fenetre
            ModifierMedecinView fenetre = new ModifierMedecinView();
            fenetre.Owner = Window.GetWindow(this);
            fenetre.ShowDialog();
        }

        public void btnSupprimerMedecin(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                try
                {
                    var valTag = btn.Tag.ToString();
                    MessageBoxResult result = MessageBox.Show(
                        "Voulez-vous supprimer ce Medecin?",
                        "Confirmation",
                        MessageBoxButton.YesNo
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        bool estSupprimer = instanceServiceMedecin.SupprimerMedecin(valTag);
                        if (estSupprimer)
                        {
                            RechargerListeMedecin();
                            MessageBox.Show("Suppression Effecutuee!");
                        } 
                        else
                        {
                            MessageBox.Show("Erreur de Suppression!");
                        }
                    }

                    
                } 
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur de Suppression: " + ex.Message);
                }
            }
        }
	}
}