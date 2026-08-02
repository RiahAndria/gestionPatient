using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Consultation
{
    public partial class ConsultationView : UserControl
    {
        // --- Instanciation des services
        private readonly ConsultationService _consultationService;
        private readonly RappelService _rappelService;
        private readonly RendezVousService _rendezVousService;

        public ConsultationView()
        {
            InitializeComponent();
            _consultationService = new ConsultationService();
            _rappelService = new RappelService();
            _rendezVousService = new RendezVousService();

            ChargerRendezVousDisponibles();
        }

        // Seuls les rendez-vous encore "Planifie" peuvent recevoir une
        // consultation (un rendez-vous deja Termine ou Annule ne doit
        // plus apparaitre ici).
        private void ChargerRendezVousDisponibles()
        {
            CbRendezVous.ItemsSource = _rendezVousService.Rechercher(terme: "", date: null, statut: "PLANIFIE");
        }

        private void CbRendezVous_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbRendezVous.SelectedItem is not RendezVousAffichage rdv)
            {
                LblDetailRdv.Text = "";
                return;
            }

            LblDetailRdv.Text = $"{rdv.MedecinNom} — {rdv.DateHeure:dd/MM/yyyy HH:mm} — {rdv.Motif}";
        }

        // --- Action lors du clic sur "Enregistrer la Consultation"
        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (CbRendezVous.SelectedItem is not RendezVousAffichage rdvSelectionne)
            {
                MessageBox.Show("Sélectionne le rendez-vous concerné par cette consultation.",
                                "Champ manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ... Validation basique des champs obligatoires
            if (string.IsNullOrWhiteSpace(TxtNumeroConsultation.Text) || string.IsNullOrWhiteSpace(TxtDiagnostique.Text))
            {
                MessageBox.Show("Veuillez remplir au moins le numéro de consultation et le diagnostic.",
                                "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var consultation = new Models.Consultation
            {
                NumeroConsultation = TxtNumeroConsultation.Text.Trim(),
                NumeroRdv = rdvSelectionne.NumeroRdv,
                NumeroDossier = TxtNumeroDossier.Text.Trim(),
                Diagnostique = TxtDiagnostique.Text.Trim(),
                NotesMedicales = TxtNotesMedicales.Text.Trim(),
                GroupeSanguin = TxtGroupeSanguin.Text.Trim(),
                Allergies = TxtAllergies.Text.Trim(),
                Traitement = TxtTraitementDossier.Text.Trim(),
                Antecedents = TxtAntecedents.Text.Trim()
            };

            if (decimal.TryParse(TxtPoids.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var poids))
            {
                consultation.Poids = poids;
            }

            if (decimal.TryParse(TxtTaille.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var taille))
            {
                consultation.Taille = taille;
            }

            // ... Création de l'Ordonnance [uniquement si les champs sont remplis]
            Ordonnance? ordonnance = null;
            if (!string.IsNullOrWhiteSpace(TxtNumeroPrescription.Text) && !string.IsNullOrWhiteSpace(TxtTraitement.Text))
            {
                ordonnance = new Ordonnance
                {
                    NumeroPrescritption = TxtNumeroPrescription.Text.Trim(),
                    NumeroConsultation = consultation.NumeroConsultation,
                    Traitement = TxtTraitement.Text.Trim(),
                    Duree = TxtDuree.Text.Trim(),
                    Diagnostique = consultation.Diagnostique
                };
            }

            var resultat = _consultationService.EnregistrerConsultation(consultation, ordonnance);

            if (!resultat.Succes)
            {
                MessageBox.Show($"Une erreur est survenue lors de l'enregistrement en base de données : {resultat.MessageErreur}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string messageFacture = resultat.FactureCreee
                ? $"Facture générée automatiquement pour {resultat.MontantFacture:N0} Ar (visible dans l'onglet Paiements)."
                : (resultat.MessageFacture ?? "Aucune facture supplémentaire à générer.");

            MessageBox.Show(
                $"La consultation a été enregistrée et le dossier médical a été synchronisé avec succès !\n\n{messageFacture}",
                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            ReinitialiserChamps();
            ChargerRendezVousDisponibles(); // le RDV utilise ne doit plus apparaitre dans la liste
        }

        //--- Action lors du clic sur "Générer Rappels RDV"
        private void BtnRappels_Click(object sender, RoutedEventArgs e)
        {
            int nbRappels = _rappelService.GenererRappels24h();

            MessageBox.Show($"{nbRappels} notification(s) de rappel de rendez-vous générée(s) pour les prochaines 24h.",
                            "Rappels RDV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //--- Méthode utilitaire pour vider le formulaire après enregistrement
        private void ReinitialiserChamps()
        {
            TxtNumeroConsultation.Clear();
            TxtNumeroDossier.Clear();
            TxtDiagnostique.Clear();
            TxtNotesMedicales.Clear();
            TxtPoids.Clear();
            TxtTaille.Clear();
            TxtGroupeSanguin.Clear();
            TxtAllergies.Clear();
            TxtTraitementDossier.Clear();
            TxtAntecedents.Clear();
            TxtNumeroPrescription.Clear();
            TxtTraitement.Clear();
            TxtDuree.Clear();
            CbRendezVous.SelectedIndex = -1;
            LblDetailRdv.Text = "";
        }
    }
}
