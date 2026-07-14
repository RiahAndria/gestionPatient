using System;
using System.Windows.Controls;

namespace Patients.Views.DossierMedical;
public partial class DossierView : UserControl
{
    public DossierView()
    {
        InitializeComponent();
    }

    private void BtnTest_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Logique pour le bouton Test
        System.Windows.MessageBox.Show("Bouton Test cliqué !");
    }
}
