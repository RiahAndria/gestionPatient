//on crée un rendez-vous pour un patient avec un médecin à une date et heure précises
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;
//création de la vue pour le formulaire de rendez-vous
public partial class RendezVousFormView : UserControl
{
    private readonly RendezVousService _rendezVousService = new();
    private readonly PatientLookupService _patients = new();
    private readonly MedecinLookupService _medecins = new();
// Constructeur de la vue
    public RendezVousFormView()
    {
        InitializeComponent();
        cbPatient.ItemsSource = _patients.Rechercher("");
        cbMedecin.ItemsSource = _medecins.ObtenirDisponibles();
    }
// Méthode pour gérer le changement de texte dans la zone de recherche du patient
    private void txtRecherchePatient_TextChanged(object sender, TextChangedEventArgs e)
    {
        cbPatient.ItemsSource = _patients.Rechercher(txtRecherchePatient.Text);
        if (cbPatient.Items.Count > 0) cbPatient.SelectedIndex = 0;
    }
// Méthode pour gérer le clic sur le bouton "Créer"
    private void btnCreer_Click(object sender, RoutedEventArgs e)
    {
        if (cbPatient.SelectedItem is not PatientOption patient)
        {
            AfficherErreur("Sélectionne un patient.");
            return;
        }
        if (cbMedecin.SelectedItem is not MedecinOption medecin)
        {
            AfficherErreur("Sélectionne un médecin disponible.");
            return;
        }
        if (dpDate.SelectedDate is null || !TimeSpan.TryParse(txtHeure.Text, out var heure))
        {
            AfficherErreur("Renseigne une date et une heure valides (HH:mm).");
            return;
        }

        var dateHeure = dpDate.SelectedDate.Value.Date + heure;
        var numero = $"RDV-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        try
        {
            _rendezVousService.AjouterRendezVous(new Patients.Models.RendezVous
            {
                NumRendezVous = numero,
                PatientID = patient.Id,
                MedecinID = medecin.IdHer2,
                DateHeure = dateHeure,
                Motif = txtMotif.Text
            });

            txtMessage.Foreground = System.Windows.Media.Brushes.Green;
            txtMessage.Text = "Rendez-vous créé.";

            if (Window.GetWindow(this) is Window fenetre) fenetre.Close();
        }
        catch (InvalidOperationException ex)
        {
        // Gérer les exceptions spécifiques à l'ajout de rendez-vous
            AfficherErreur(ex.Message);
        }
        catch (Exception ex)
        {
            AfficherErreur($"Erreur inattendue : {ex.Message}");
        }
    }
    // Méthode pour afficher les messages d'erreur
    private void AfficherErreur(string message)
    {
        txtMessage.Foreground = System.Windows.Media.Brushes.Red;
        txtMessage.Text = message;
    }
}