using System.ComponentModel;
using Patients.Models;

namespace Patients.Views.Medecin.DetailEtDisponibilite;

public partial class MainViewModel : INotifyPropertyChanged
{
    private object? _currentViewModel;

    public Patients.Models.Medecin _donneMedecin = new Patients.Models.Medecin();

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }

    public MainViewModel(Patients.Models.Medecin medecin)
    {
        _donneMedecin = medecin;
        CurrentViewModel = new DetailMedecin(_donneMedecin);
    }

    public void AfficherEcrantDetail() => CurrentViewModel = new DetailMedecin(_donneMedecin);
    public void AfficherEcrantDashboard() => CurrentViewModel = new DashboardDispo(_donneMedecin);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

}