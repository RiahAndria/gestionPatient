using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape4CreneauMedecinView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly Action _allerPrecedent;
    private readonly DisponibiliteService _disponibilites = new();

    private readonly Dictionary<Border, CreneauBloc> _cartesParBloc = new();
    private Border? _carteBlocSelectionnee;
    private readonly DateOnly _date;

    public Etape4CreneauMedecinView(AssistantRendezVousState etat, Action allerSuivant, Action allerPrecedent)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _allerPrecedent = allerPrecedent;

        _date = DateOnly.FromDateTime(_etat.DateChoisie ?? DateTime.Today);
        TxtSousTitre.Text = $"Choisissez un créneau pour le {_date:dd/MM/yyyy}";

        ChargerCreneaux();
    }

    private void ChargerCreneaux()
    {
        List<CreneauBloc> blocs;
        try
        {
            blocs = _disponibilites.ObtenirBlocsDisponibles(_date, _etat.Service?.CodeFonction ?? 0);
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Impossible de charger les créneaux : {ex.Message}";
            return;
        }

        if (blocs.Count == 0)
        {
            TxtErreur.Text = "Aucun créneau disponible à cette date pour ce service. Reviens à l'étape précédente pour choisir une autre date.";
            return;
        }

        foreach (var bloc in blocs)
        {
            var carte = CreerCarteBloc(bloc);
            _cartesParBloc[carte] = bloc;
            PanelCreneaux.Children.Add(carte);

            if (_etat.Creneau != null && _etat.Creneau.NumeroBloc == bloc.NumeroBloc)
            {
                SelectionnerBloc(carte, bloc, restaurerMedecin: true);
            }
        }
    }

    private Border CreerCarteBloc(CreneauBloc bloc)
    {
        var carte = new Border
        {
            Width = 190,
            Height = 46,
            Margin = new Thickness(0, 0, 12, 0),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = (Brush)FindResource("BrushBorder"),
            Background = (Brush)FindResource("BrushSurface"),
            Cursor = Cursors.Hand,
            Child = new TextBlock { Text = bloc.Libelle, FontSize = 12, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };
        carte.MouseLeftButtonUp += (_, _) => SelectionnerBloc(carte, bloc, restaurerMedecin: false);
        return carte;
    }

    private void SelectionnerBloc(Border carte, CreneauBloc bloc, bool restaurerMedecin)
    {
        if (_carteBlocSelectionnee != null)
        {
            _carteBlocSelectionnee.BorderBrush = (Brush)FindResource("BrushBorder");
            _carteBlocSelectionnee.Background = (Brush)FindResource("BrushSurface");
        }

        carte.BorderBrush = (Brush)FindResource("BrushPrimary");
        carte.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
        _carteBlocSelectionnee = carte;

        _etat.Creneau = bloc;
        if (!restaurerMedecin) _etat.Medecin = null;

        ChargerMedecins(bloc, restaurerMedecin);
    }

    private void ChargerMedecins(CreneauBloc bloc, bool restaurerMedecin)
    {
        TxtSousTitreMedecin.Visibility = Visibility.Visible;
        TxtErreur.Text = "";
        BtnSuivant.IsEnabled = false;

        List<MedecinDisponible> medecins;
        try
        {
            medecins = _disponibilites.ObtenirMedecinsDisponibles(_date, _etat.Service?.CodeFonction ?? 0, bloc.NumeroBloc);
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Impossible de charger les médecins : {ex.Message}";
            return;
        }

        ListeMedecins.ItemsSource = medecins;

        if (medecins.Count == 0)
        {
            TxtErreur.Text = "Aucun médecin de ce service n'est disponible sur ce créneau.";
            return;
        }

        if (restaurerMedecin && _etat.Medecin != null)
        {
            ListeMedecins.SelectedItem = medecins.Find(m => m.Id == _etat.Medecin.Id);
        }
    }

    private void ListeMedecins_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListeMedecins.SelectedItem is MedecinDisponible medecin)
        {
            _etat.Medecin = medecin;
            BtnSuivant.IsEnabled = true;
            TxtErreur.Text = "";
        }
        else
        {
            BtnSuivant.IsEnabled = false;
        }
    }

    private void BtnPrecedent_Click(object sender, RoutedEventArgs e) => _allerPrecedent();

    private void BtnSuivant_Click(object sender, RoutedEventArgs e)
    {
        if (_etat.Creneau is null || _etat.Medecin is null)
        {
            TxtErreur.Text = "Choisis un créneau et un médecin pour continuer.";
            return;
        }

        _allerSuivant();
    }
}
