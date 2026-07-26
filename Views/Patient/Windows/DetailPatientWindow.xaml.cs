using System.Windows;
using System.Windows.Controls;
using Patients.Models;

namespace Patients.Views.Patient;

public partial class DetailPatientWindow : Window
{
    public DetailPatientWindow(Models.Patient? patient, Dossier? dossier = null)
    {
        InitializeComponent();

        if (patient is not null)
        {
            // Informations Identité
            if (FindName("lblIdentite") is TextBlock identiteLabel)
                identiteLabel.Text = $"{patient.Nom?.ToUpper()} {patient.Prenom}";

            if (FindName("lblMatricule") is TextBlock matriculeLabel)
                matriculeLabel.Text = $"DOSSIER N° {patient.NumeroDossier}";

            // Contact & Assurance
            if (FindName("lblContact") is TextBlock contactLabel)
                contactLabel.Text = $" {patient.Telephone}\n {patient.Email}\n {patient.Adresse}";

            if (FindName("lblAssurance") is TextBlock assuranceLabel)
                assuranceLabel.Text = string.IsNullOrWhiteSpace(patient.NumeroAssurance)
                    ? " Sans assurance"
                    : $" Assur: {patient.NumeroAssurance}";
        }

        // Remplissage à partir du modèle Dossier.cs
        if (dossier != null)
        {
            // Poids et Taille sont des decimals dans Dossier.cs
            if (FindName("lblPoids") is TextBlock poidsLabel)
                poidsLabel.Text = $"{dossier.Poids} kg";

            if (FindName("lblTaille") is TextBlock tailleLabel)
                tailleLabel.Text = $"{dossier.Taille} cm";

            if (FindName("lblGroupe") is TextBlock groupeLabel)
                groupeLabel.Text = string.IsNullOrWhiteSpace(dossier.GroupeSanguin) ? "-" : dossier.GroupeSanguin;

            if (FindName("txtTraitements") is TextBlock traitementsLabel)
                traitementsLabel.Text = string.IsNullOrWhiteSpace(dossier.Traitement)
                    ? "Pas de traitement actif"
                    : dossier.Traitement;

            if (FindName("txtAllergies") is TextBlock allergiesLabel)
                allergiesLabel.Text = string.IsNullOrWhiteSpace(dossier.Allergies)
                    ? "Aucune allergie connue"
                    : dossier.Allergies;

            if (FindName("txtAntecedents") is TextBlock antecedentsLabel)
                antecedentsLabel.Text = string.IsNullOrWhiteSpace(dossier.Antecedents)
                    ? "Aucun antécédent répertorié"
                    : dossier.Antecedents;
        }
        else
        {
            // Affichage par défaut si le dossier médical n'a pas encore été renseigné
            if (FindName("lblPoids") is TextBlock poidsLabel)
                poidsLabel.Text = "-- kg";

            if (FindName("lblTaille") is TextBlock tailleLabel)
                tailleLabel.Text = "-- cm";

            if (FindName("lblGroupe") is TextBlock groupeLabel)
                groupeLabel.Text = "--";

            if (FindName("txtTraitements") is TextBlock traitementsLabel)
                traitementsLabel.Text = "Aucun dossier médical enregistré.";

            if (FindName("txtAllergies") is TextBlock allergiesLabel)
                allergiesLabel.Text = "Aucune donnée.";

            if (FindName("txtAntecedents") is TextBlock antecedentsLabel)
                antecedentsLabel.Text = "Aucune donnée.";
        }
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}