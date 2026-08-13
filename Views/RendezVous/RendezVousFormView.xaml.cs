using System.Windows;
using System.Windows.Controls;

namespace Patients.Views.RendezVous
{
    public partial class RendezVousFormView : UserControl
    {
        public RendezVousFormView()
        {
            InitializeComponent();
        }

        private void txtRecherchePatient_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ancien formulaire conservé pour compatibilité.
            // Le filtrage n'est pas géré ici si le contrôle est utilisé dans l'ancien flux.
        }

        private void btnCreer_Click(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = "La création de rendez-vous n'est pas disponible dans ce formulaire hérité.";
        }
    }
}
