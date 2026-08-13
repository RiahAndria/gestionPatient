using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Patients.Services;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape1SelectionPatientView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly PatientService _patientService;
    private List<Patients.Models.Patient> _tousLesPatients = new();

    public Etape1SelectionPatientView(AssistantRendezVousState etat, Action allerSuivant)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _patientService = new PatientService();

        try
        {
            _tousLesPatients = _patientService.ObtenirTousLesPatients();
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Impossible de charger les patients : {ex.Message}";
        }

        // Si l'assistant a ete ouvert depuis la fiche d'un patient
        // precis, on saute directement a la liste avec sa fiche
        // preselectionnee (comportement equivalent a l'ancien
        // RendezVousFormView(patientId)).
        if (!string.IsNullOrWhiteSpace(_etat.PatientIdPreselectionne))
        {
            AfficherListe();
            var patient = _tousLesPatients.FirstOrDefault(p => p.Id == _etat.PatientIdPreselectionne);
            if (patient != null)
            {
                ListePatients.SelectedItem = patient;
            }
        }
    }

    private void PanelIntro_Click(object sender, MouseButtonEventArgs e) => AfficherListe();

    private void Retour_Click(object sender, MouseButtonEventArgs e)
    {
        PanelListe.Visibility = Visibility.Collapsed;
        PanelIntro.Visibility = Visibility.Visible;
    }

    private void AfficherListe()
    {
        PanelIntro.Visibility = Visibility.Collapsed;
        PanelListe.Visibility = Visibility.Visible;
        ListePatients.ItemsSource = _tousLesPatients;
    }

    private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
    {
        string terme = TxtRecherche.Text?.Trim() ?? "";
        ListePatients.ItemsSource = string.IsNullOrEmpty(terme)
            ? _tousLesPatients
            : _tousLesPatients.Where(p =>
                p.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                p.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase) ||
                p.NumeroDossier.Contains(terme, StringComparison.OrdinalIgnoreCase))
              .ToList();
    }

    private void ListePatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BtnSuivant.IsEnabled = ListePatients.SelectedItem is Patients.Models.Patient;
        TxtErreur.Text = "";
    }

    private void BtnSuivant_Click(object sender, RoutedEventArgs e)
    {
        if (ListePatients.SelectedItem is not Patients.Models.Patient patient)
        {
            TxtErreur.Text = "Sélectionne un patient pour continuer.";
            return;
        }

        _etat.Patient = patient;
        _allerSuivant();
    }
}
