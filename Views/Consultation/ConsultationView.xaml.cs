using System;
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

        public ConsultationView()
        {
            InitializeComponent();
            _consultationService = new ConsultationService();
            _rappelService = new RappelService();
        }

        // --- Action lors du clic sur "Enregistrer la Consultation"
        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            // ... Validation basique des champs obligatoires
            if (string.IsNullOrWhiteSpace(TxtNumeroConsultation.Text) || string.IsNullOrWhiteSpace(TxtDiagnostique.Text))
            {
                MessageBox.Show("Veuillez remplir au moins le numéro de consultation et le diagnostic.", 
                                "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ... Création de l'objet Consultation
            var consultation = new Models.Consultation
            {
                NumeroConsultation = TxtNumeroConsultation.Text.Trim(),
                Diagnostique = TxtDiagnostique.Text.Trim(),
                NotesMedicales = TxtNotesMedicales.Text.Trim()
            };

            // ... Création de l'Ordonnance [uniquement si les champs sont remplis]
            Ordonnance? ordonnance = null;
            if (!string.IsNullOrWhiteSpace(TxtNumeroPrescription.Text) && !string.IsNullOrWhiteSpace(TxtTraitement.Text))
            {
                ordonnance = new Ordonnance
                {
                    NumeroPrescription = TxtNumeroPrescription.Text.Trim(),
                    NumeroConsultation = consultation.NumeroConsultation,
                    Traitement = TxtTraitement.Text.Trim(),
                    Duree = TxtDuree.Text.Trim(),
                    Diagnostique = consultation.Diagnostique
                };
            }

            // ... Appel du service Back-End (Correction du nom de la méthode)
            bool succes = _consultationService.EnregistrerConsultation(consultation, ordonnance);

            if (succes)
            {
                MessageBox.Show("La consultation et la prescription ont été enregistrées avec succès !", 
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                ReinitialiserChamps();
            }
            else
            {
                MessageBox.Show("Une erreur est survenue lors de l'enregistrement en base de données.", 
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            TxtDiagnostique.Clear();
            TxtNotesMedicales.Clear();
            TxtNumeroPrescription.Clear();
            TxtTraitement.Clear();
            TxtDuree.Clear();
        }
    }
}