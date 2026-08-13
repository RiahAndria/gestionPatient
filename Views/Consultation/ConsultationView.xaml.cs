using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Consultation
{
    public partial class ConsultationView : UserControl
    {
        private readonly ConsultationService _consultationService;
        private readonly RappelService _rappelService;
        private readonly RendezVousService _rendezVousService;

        public ConsultationView()
        {
            InitializeComponent();
            _consultationService = new ConsultationService();
            _rappelService = new RappelService();
            _rendezVousService = new RendezVousService();

            CbGroupeSanguin.ItemsSource = new string[] { "Inconnu", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            CbGroupeSanguin.SelectedIndex = 0;
            ChargerRdv();
        }

        private void ChargerRdv()
        {
            CbRendezVous.ItemsSource = _rendezVousService.Rechercher("", null, "PLANIFIE");
        }

        private void CbRendezVous_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbRendezVous.SelectedItem is not RendezVousAffichage rdv)
            {
                LblDetailRdv.Text = "👤 Aucun patient sélectionné";
                TxtNumeroDossier.Text = "Aucun dossier affecté";
                TxtNumeroConsultation.Text = "Aucun numéro affecté";
                TxtNumeroPrescription.Text = "Aucune ordonnance affectée";
                return;
            }

            // En-tête Patient explicite
            LblDetailRdv.Text = $"👤 PATIENT : {rdv.PatientNom.ToUpper()} | 👨‍⚕️ Dr. {rdv.MedecinNom} | 📅 {rdv.DateHeure:dd/MM/yyyy à HH:mm}";

            // Auto-génération stricte des numéros
            TxtNumeroDossier.Text = $"DOS-{rdv.NumeroRdv}";
            TxtNumeroConsultation.Text = $"CS-{DateTime.Now:yyyyMMdd}-{rdv.NumeroRdv}";
            TxtNumeroPrescription.Text = $"ORD-{DateTime.Now:yyyyMMdd}-{rdv.NumeroRdv}";
        }

        // Restreint la saisie aux nombres décimaux (ex: Poids, Taille, Température)
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+([.,][0-9]*)?$");
            string proposedText = (sender is TextBox tb) ? tb.Text.Insert(tb.CaretIndex, e.Text) : e.Text;
            e.Handled = !regex.IsMatch(proposedText);
        }

        // Restreint la saisie aux entiers stricts (ex: Pouls, Durée en jours)
        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void List_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "- ";
                tb.CaretIndex = 2;
            }
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb)
            {
                int idx = tb.CaretIndex;
                tb.Text = tb.Text.Insert(idx, "\n- ");
                tb.CaretIndex = idx + 3;
                e.Handled = true;
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (CbRendezVous.SelectedItem is not RendezVousAffichage rdv)
            {
                Warn("Veuillez sélectionner un rendez-vous avant d'enregistrer.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDiagnostique.Text))
            {
                Warn("Le diagnostic est obligatoire.");
                return;
            }

            var consultation = new Patients.Models.Consultation
            {
                NumeroConsultation = TxtNumeroConsultation.Text.Trim(),
                NumeroRdv = rdv.NumeroRdv,
                NumeroDossier = TxtNumeroDossier.Text.Trim(),
                Diagnostique = TxtDiagnostique.Text.Trim(),
                NotesMedicales = TxtNotesMedicales.Text.Trim(),
                GroupeSanguin = CbGroupeSanguin.SelectedItem?.ToString() ?? "Inconnu",
                Allergies = TxtAllergies.Text.Trim(),
                Traitement = TxtTraitementDossier.Text.Trim(),
                Antecedents = TxtAntecedents.Text.Trim()
            };

            if (decimal.TryParse(TxtPoids.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal p))
                consultation.Poids = p;

            if (decimal.TryParse(TxtTaille.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal t))
                consultation.Taille = t;

            Patients.Models.Ordonnance? ord = null;
            if (!string.IsNullOrWhiteSpace(TxtTraitement.Text))
            {
                string dureeTexte = string.IsNullOrWhiteSpace(TxtDuree.Text) ? "" : $"{TxtDuree.Text.Trim()} jours";
                ord = new Patients.Models.Ordonnance
                {
                    NumeroPrescritption = TxtNumeroPrescription.Text.Trim(),
                    NumeroConsultation = consultation.NumeroConsultation,
                    Traitement = TxtTraitement.Text.Trim(),
                    Duree = dureeTexte,
                    Diagnostique = consultation.Diagnostique
                };
            }

            var res = _consultationService.EnregistrerConsultation(consultation, ord);
            if (!res.Succes)
            {
                MessageBox.Show($"Erreur SQL : {res.MessageErreur}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string msgFact = res.FactureCreee ? $"Facture générée : {res.MontantFacture:N0} Ar." : (res.MessageFacture ?? "");
            MessageBox.Show($"Consultation enregistrée avec succès !\n\n{msgFact}", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            ResetForm();
            ChargerRdv();
        }

        private void BtnRappels_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"{_rappelService.GenererRappels24h()} rappel(s) généré(s).", "Rappels", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetForm()
        {
            TxtNumeroConsultation.Text = "Aucun numéro affecté";
            TxtNumeroDossier.Text = "Aucun dossier affecté";
            TxtNumeroPrescription.Text = "Aucune ordonnance affectée";

            TxtDiagnostique.Clear();
            TxtNotesMedicales.Clear();
            TxtPoids.Clear();
            TxtTaille.Clear();
            TxtTemperature.Clear();
            TxtFrequenceCardiaque.Clear();
            TxtAllergies.Clear();
            TxtTraitementDossier.Clear();
            TxtAntecedents.Clear();
            TxtTraitement.Clear();
            TxtDuree.Clear();

            CbGroupeSanguin.SelectedIndex = 0;
            CbRendezVous.SelectedIndex = -1;
            LblDetailRdv.Text = "👤 Aucun patient sélectionné";
        }

        private static void Warn(string msg)
        {
            MessageBox.Show(msg, "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}