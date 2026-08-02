using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous;

public partial class RendezVousFormView : UserControl
{
    private readonly RendezVousService _rendezVousService = new();
    private readonly PatientLookupService _patients = new();
    private readonly MedecinLookupService _medecins = new();

    public RendezVousFormView(string? patientId = null)
    {
        InitializeComponent();
        var patientOptions = _patients.Rechercher("");
        cbPatient.ItemsSource = patientOptions;
        cbMedecin.ItemsSource = _medecins.ObtenirDisponibles();

        if (!string.IsNullOrWhiteSpace(patientId) && patientOptions.Any())
        {
            cbPatient.SelectedItem = patientOptions.FirstOrDefault(p => p.Id == patientId);
        }

        if (cbPatient.SelectedItem == null && cbPatient.Items.Count > 0)
        {
            cbPatient.SelectedIndex = 0;
        }
    }

    private void txtRecherchePatient_TextChanged(object sender, TextChangedEventArgs e)
    {
        cbPatient.ItemsSource = _patients.Rechercher(txtRecherchePatient.Text);
        if (cbPatient.Items.Count > 0) cbPatient.SelectedIndex = 0;
    }

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
        var dateHeureSaisie = dpDate.SelectedDate.Value.Date + heure;

        if (dateHeureSaisie < DateTime.Now)
        {
            AfficherErreur("Impossible de créer un rendez-vous dans le passé.");
            return;
        }

        if (heure < TimeSpan.FromHours(8) || heure > TimeSpan.FromHours(18))
        {
            AfficherErreur("Les rendez-vous se prennent entre 08:00 et 18:00 (horaires d'ouverture).");
            return;
        }

        try
        {
            // Le type est precise en entier (Patients.Models.RendezVous)
            // car ce fichier est lui-meme dans le namespace
            // Patients.Views.RendezVous : sans cette precision, le
            // compilateur confond la classe RendezVous avec le
            // namespace du meme nom.
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
            // Erreur metier attendue (ex: creneau deja pris) : message clair.
            AfficherErreur(ex.Message);
        }
        catch (Exception ex)
        {
            AfficherErreur($"Erreur inattendue : {ex.Message}");
        }
    }

    private void AfficherErreur(string message)
    {
        txtMessage.Foreground = System.Windows.Media.Brushes.Red;
        txtMessage.Text = message;
    }
}
