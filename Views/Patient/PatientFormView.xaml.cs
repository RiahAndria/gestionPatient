using System.Windows;
using System.Windows.Controls;

namespace Patients.Views.Patient;

public partial class PatientFormView : UserControl
{
    public PatientFormView()
    {
        InitializeComponent();
    }

    private void btnAjouter_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNom.Text))
        {
            txtMessage.Text = "Veuillez entrer un nom.";
        }
        else if (string.IsNullOrWhiteSpace(txtPrenom.Text))
        {
            txtMessage.Text = "Veuillez entrer un prénom.";
        }
        else if (dpDateNaissance.SelectedDate == null)
        {
            txtMessage.Text = "Veuillez sélectionner une date de naissance.";
        }
        else
        {
            txtMessage.Text = "Patient ajouté avec succès !";
        }
    }
}