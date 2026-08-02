using System.ComponentModel;
using System.Configuration.Assemblies;
using System.Windows;
using System.Windows.Documents;
using Patients.Models;
using Patients.Services;
using System.Text.RegularExpressions;

namespace Patients.Views.Medecin.AjoutFonction;

public partial class AjoutFonctionView : Window
{ 
    private FonctionService _fonctionService = new FonctionService();
    public AjoutFonctionView()
    {
        InitializeComponent();
    }

    public void ajout_nouvelle_fonction(Object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TXTFonctionAjouter.Text))
        {
            MessageBox.Show("Ajout non effectue");
        }

        string normeNouvFonc = @"^[a-zA-Z]{5,20}$";
        if (!Regex.IsMatch(TXTFonctionAjouter.Text, normeNouvFonc))
        {
            MessageBox.Show("Nom fonction invalide");
            return;   
        }

        //si ok on realise l'ajout dans la base de donne 
        Fonction _fonction = new Fonction();

        _fonction.nom_fonction = TXTFonctionAjouter.Text;
        bool estAjouter = _fonctionService.AjouterNouvelleFonction(_fonction);
        if (estAjouter)
        {
            TXTMessageAjoutFonction.Text = "Ajout de fonction effectue";
        }
        else
        {
            TXTMessageAjoutFonction.Text = _fonctionService.message;
        }
    }
}