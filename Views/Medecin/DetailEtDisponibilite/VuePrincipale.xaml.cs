// using System.ComponentModel;
// using System.Net.NetworkInformation;
// using System.Runtime.CompilerServices;

// // using Patients.Views.Medecin.DetailEtDisponibilite;

// using Patients.Views.Medecin;

// public class MainViewModel : INotifyPropertyChanged
// {
//     private object _vueActuelle;

//     public object VueActuelle
//     {
//         get => _vueActuelle;
//         set
//         {
//             _vueActuelle = value;
//             OnPropertyChanged();
//         }
//     }

//     public MainViewModel()
//     {
//         VueActuelle = new AccueilViewModel();
//     }

//     public void AfficherAccueil() => VueActuelle = new AccueilViewModel();
//     public void AfficherProfil() => VueActuelle = new ProfilViewModel();

//     public event PropertyChangedEventHandler PropertyChanged;
//     protected void OnPropertyChanged([CallerMemberName] string name = null)
//     {
//         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//     } 
// }


//=======================================================================
using System;
using System.Windows;
using System.Windows.Controls;
using Patients.Models;

namespace Patients.Views.Medecin.DetailEtDisponibilite;
public partial class VuePrincipale : Window
{
    private MainViewModel _mainVM;

    public VuePrincipale(Patients.Models.Medecin medecin)
    {
        InitializeComponent();
        _mainVM = new MainViewModel(medecin);
        this.DataContext = _mainVM; // On lie le DataContext
    }

    private void BtnMedecin_Click(object sender, RoutedEventArgs e)
    {
        _mainVM.AfficherEcrantDetail(); // Change l'objet en C# !
    }

    private void BtnPatient_Click(object sender, RoutedEventArgs e)
    {
        _mainVM.AfficherEcrantDashboard(); // Change l'objet en C# !
    }
}