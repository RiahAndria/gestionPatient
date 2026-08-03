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
        dgHistorique.ItemsSource = consultations;
    }
}
