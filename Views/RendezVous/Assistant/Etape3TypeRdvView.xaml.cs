using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape3TypeRdvView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly Action _allerPrecedent;

    private readonly bool _serviceNecessiteDelai;
    private readonly DateTime _dateMinimumPlanification;

    public Etape3TypeRdvView(AssistantRendezVousState etat, Action allerSuivant, Action allerPrecedent)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _allerPrecedent = allerPrecedent;

        _serviceNecessiteDelai = _etat.Service?.NecessiteDelai ?? true;

        // Regle metier : un service qui necessite un delai n'ouvre le
        // calendrier qu'a partir de J+2 minimum. Sinon, la
        // planification reste possible des demain (aujourd'hui est
        // deja couvert par l'option "Aujourd'hui").
        _dateMinimumPlanification = DateTime.Today.AddDays(_serviceNecessiteDelai ? 2 : 1);
        DpDate.DisplayDateStart = _dateMinimumPlanification;

        if (_serviceNecessiteDelai)
        {
            CarteAujourdHui.IsEnabled = false;
            CarteAujourdHui.Opacity = 0.5;
            TxtInfoDelai.Text = "Ce service nécessite un rendez-vous à l'avance : l'option « Aujourd'hui » n'est pas disponible.";
        }

        // Si on revient en arriere depuis une etape suivante avec un
        // choix deja fait, on le restaure.
        if (_etat.RdvAujourdHui)
        {
            SelectionnerAujourdHui();
        }
        else if (_etat.DateChoisie.HasValue)
        {
            SelectionnerPlanifier();
            DpDate.SelectedDate = _etat.DateChoisie;
        }
    }

    private void CarteAujourdHui_Click(object sender, MouseButtonEventArgs e)
    {
        if (!CarteAujourdHui.IsEnabled) return;
        SelectionnerAujourdHui();
    }

    private void SelectionnerAujourdHui()
    {
        MettreEnSurbrillance(CarteAujourdHui);
        DpDate.Visibility = Visibility.Collapsed;

        _etat.RdvAujourdHui = true;
        _etat.DateChoisie = DateTime.Today;

        BtnSuivant.IsEnabled = true;
        TxtErreur.Text = "";
    }

    private void CartePlanifier_Click(object sender, MouseButtonEventArgs e) => SelectionnerPlanifier();

    private void SelectionnerPlanifier()
    {
        MettreEnSurbrillance(CartePlanifier);
        DpDate.Visibility = Visibility.Visible;

        _etat.RdvAujourdHui = false;
        // Le clic seul ne suffit pas : il faut encore choisir une date
        // (voir DpDate_SelectedDateChanged) avant d'activer "Suivant".
        BtnSuivant.IsEnabled = _etat.DateChoisie.HasValue && _etat.DateChoisie.Value.Date >= _dateMinimumPlanification;
    }

    private void MettreEnSurbrillance(Border carteChoisie)
    {
        foreach (var carte in new[] { CarteAujourdHui, CartePlanifier })
        {
            bool estChoisie = carte == carteChoisie;
            carte.BorderBrush = (Brush)FindResource(estChoisie ? "BrushPrimary" : "BrushBorder");
            carte.Background = estChoisie
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF))
                : (Brush)FindResource("BrushSurface");
        }
    }

    private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DpDate.SelectedDate is null) return;

        if (DpDate.SelectedDate.Value.Date < _dateMinimumPlanification)
        {
            TxtErreur.Text = $"La date la plus proche disponible pour ce service est le {_dateMinimumPlanification:dd/MM/yyyy}.";
            BtnSuivant.IsEnabled = false;
            return;
        }

        TxtErreur.Text = "";
        _etat.DateChoisie = DpDate.SelectedDate.Value.Date;
        BtnSuivant.IsEnabled = true;
    }

    private void BtnPrecedent_Click(object sender, RoutedEventArgs e) => _allerPrecedent();

    private void BtnSuivant_Click(object sender, RoutedEventArgs e)
    {
        if (!_etat.DateChoisie.HasValue)
        {
            TxtErreur.Text = "Choisis « Aujourd'hui » ou une date de planification.";
            return;
        }

        // Le medecin/creneau de l'etape suivante dependent de la date :
        // si on revient modifier la date, on invalide le choix precedent.
        _etat.Creneau = null;
        _etat.Medecin = null;

        _allerSuivant();
    }
}
