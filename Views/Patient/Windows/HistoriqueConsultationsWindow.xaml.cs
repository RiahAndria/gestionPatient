using System;
using System.Collections.Generic;
using System.Windows;
using Patients.Models;

namespace Patients.Views.Patient.Windows;

public partial class HistoriqueConsultationsWindow : Window
{
    public HistoriqueConsultationsWindow(Models.Patient patient, IEnumerable<Patients.Models.Consultation> consultations)
    {
        InitializeComponent();
        txtPatient.Text = $"Patient : {patient.Nom} {patient.Prenom}  •  Dossier : {patient.NumeroDossier}";

        var listeConsultations = consultations?.ToList() ?? new List<Patients.Models.Consultation>();

        if (listeConsultations.Count == 0)
        {
            dgHistorique.Visibility = Visibility.Collapsed;
            txtAucuneConsultation.Visibility = Visibility.Visible;
            return;
        }

        dgHistorique.ItemsSource = listeConsultations;
        dgHistorique.Visibility = Visibility.Visible;
        txtAucuneConsultation.Visibility = Visibility.Collapsed;
    }
}
