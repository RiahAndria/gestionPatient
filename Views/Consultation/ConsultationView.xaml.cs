using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.Consultation
{
    public partial class ConsultationView : UserControl
    {
        private readonly ConsultationService _consultationService;
        private readonly RappelService _rappelService;
        private readonly RendezVousService _rendezVousService;
        private readonly PaiementService _paiementService;

        public ConsultationView()
        {
            InitializeComponent();
            _consultationService = new ConsultationService();
            _rappelService = new RappelService();
            _rendezVousService = new RendezVousService();
            _paiementService = new PaiementService();

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

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (CbRendezVous.SelectedItem is not RendezVousAffichage rdvSelectionne)
            {
                AfficherMessage("Sélectionne le rendez-vous concerné par cette consultation.", succes: false);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNumeroConsultation.Text) || string.IsNullOrWhiteSpace(TxtDiagnostique.Text))
            {
                AfficherMessage("Veuillez remplir au moins le numéro de consultation et le diagnostic.", succes: false);
                return;
            }

            var consultation = new Models.Consultation
            {
                NumeroConsultation = TxtNumeroConsultation.Text.Trim(),
                NumeroRdv = rdvSelectionne.NumeroRdv,
                Diagnostique = TxtDiagnostique.Text.Trim(),
                NotesMedicales = TxtNotesMedicales.Text.Trim(),
                GroupeSanguin = TxtGroupeSanguin.Text.Trim(),
                Allergies = TxtAllergies.Text.Trim(),
                Traitement = TxtTraitementDossier.Text.Trim(),
                Antecedents = TxtAntecedents.Text.Trim()
            };

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
                AfficherMessage($"Erreur lors de l'enregistrement : {resultat.MessageErreur}", succes: false);
                return;
            }

            if (resultat.FactureCreee)
            {
                AfficherMessage(
                    $"Consultation enregistrée. Facture générée automatiquement pour {resultat.MontantFacture:N0} Ar (visible dans l'onglet Paiements).",
                    succes: true);
            }
            else
            {
                AfficherMessage(
                    $"Consultation enregistrée, mais la facture n'a pas pu être créée automatiquement ({resultat.MessageErreur}). À créer manuellement si besoin.",
                    succes: false);
            }

            ReinitialiserChamps();
            ChargerRendezVousDisponibles(); // le RDV utilise ne doit plus apparaitre dans la liste
        }

        private void BtnRappels_Click(object sender, RoutedEventArgs e)
        {
            int nbRappels = _rappelService.GenererRappels24h();

            MessageBox.Show($"{nbRappels} notification(s) de rappel de rendez-vous générée(s) pour les prochaines 24h.",
                            "Rappels RDV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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

        private void AfficherMessage(string message, bool succes)
        {
            LblMessage.Foreground = succes ? new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)) : Brushes.Red;
            LblMessage.Text = message;
        }
    }
}
