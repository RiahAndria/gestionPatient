using System.Windows;

namespace Patients.Views.RendezVous;

public partial class ChangerStatutWindow : Window
{
    public string? StatutChoisi { get; private set; }

    public ChangerStatutWindow(string statutActuel)
    {
        InitializeComponent();

        switch (statutActuel)
        {
            case "PLANIFIE": RadioPlanifie.IsChecked = true; break;
            case "TERMINE": RadioTermine.IsChecked = true; break;
            case "ANNULE": RadioAnnule.IsChecked = true; break;
        }
    }

    private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
    {
        StatutChoisi = RadioPlanifie.IsChecked == true ? "PLANIFIE"
            : RadioTermine.IsChecked == true ? "TERMINE"
            : RadioAnnule.IsChecked == true ? "ANNULE"
            : null;

        DialogResult = StatutChoisi != null;
        Close();
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
