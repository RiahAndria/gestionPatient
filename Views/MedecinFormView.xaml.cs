using System.Windows;
using System.Windows.Controls;

namespace Patients.Views;

public partial class MedecinFormView : UserControl
{
    public MedecinFormView()
    {
        InitializeComponent();
    }

    private void btnAjouterMedecin_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNomMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer un nom.";
        }
        else if (string.IsNullOrWhiteSpace(txtPrenomMedecin.Text))
        {
            txtMessageMedecin.Text = "Veuillez entrer un prénom.";
        }
        else if (dpDateNaissanceMedecin.SelectedDate == null)
        {
            txtMessageMedecin.Text = "Veuillez sélectionner une date de naissance.";
        }
        else
        {
            txtMessageMedecin.Text = "Médecin ajouté avec succès !";
        }
    }
}