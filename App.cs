using System.Windows;

namespace Patients;
// Classe principale de l'application WPF
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Attrape TOUTE erreur non geree dans l'application (pas
        // seulement au demarrage) et l'affiche dans une popup, au lieu
        // de laisser l'appli se fermer silencieusement 
        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show(
                args.Exception.ToString(),
                "Erreur non gérée",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            args.Handled = true; // empeche le crash silencieux
        };
    }
}