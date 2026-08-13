using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Patients.Models;
using Patients.Services;

namespace Patients.Views.RendezVous.Assistant;

public partial class Etape2ChoixServiceView : UserControl
{
    private readonly AssistantRendezVousState _etat;
    private readonly Action _allerSuivant;
    private readonly Action _allerPrecedent;
    private readonly ServiceMedicalLookupService _services = new();

    private readonly Dictionary<Border, ServiceMedical> _cartesParService = new();
    private Border? _carteSelectionnee;

    public Etape2ChoixServiceView(AssistantRendezVousState etat, Action allerSuivant, Action allerPrecedent)
    {
        InitializeComponent();
        _etat = etat;
        _allerSuivant = allerSuivant;
        _allerPrecedent = allerPrecedent;

        ChargerServices();
    }

    private void ChargerServices()
    {
        List<ServiceMedical> services;
        try
        {
            services = _services.ObtenirServicesDisponibles();
        }
        catch (Exception ex)
        {
            TxtErreur.Text = $"Impossible de charger les services : {ex.Message}";
            return;
        }

        foreach (var service in services)
        {
            var carte = CreerCarteService(service);
            _cartesParService[carte] = service;
            PanelServices.Children.Add(carte);

            // Si on revient en arriere depuis une etape suivante, on
            // remet en surbrillance le service deja choisi.
            if (_etat.Service != null && _etat.Service.CodeFonction == service.CodeFonction)
            {
                SelectionnerCarte(carte, service);
            }
        }
    }

    private Border CreerCarteService(ServiceMedical service)
    {
        var titre = new TextBlock { Text = service.NomService, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushTextPrimary") };
        var indication = new TextBlock
        {
            Text = service.Indication,
            FontSize = 11,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = service.NecessiteDelai ? (Brush)FindResource("BrushWarning") : (Brush)FindResource("BrushSuccess")
        };

        var contenu = new StackPanel();
        contenu.Children.Add(titre);
        contenu.Children.Add(indication);

        var carte = new Border
        {
            Width = 190,
            Height = 90,
            Margin = new Thickness(0, 0, 12, 12),
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            BorderBrush = (Brush)FindResource("BrushBorder"),
            Background = (Brush)FindResource("BrushSurface"),
            Cursor = Cursors.Hand,
            Child = contenu
        };
        carte.MouseLeftButtonUp += (_, _) => SelectionnerCarte(carte, service);

        return carte;
    }

    private void SelectionnerCarte(Border carte, ServiceMedical service)
    {
        if (_carteSelectionnee != null)
        {
            _carteSelectionnee.BorderBrush = (Brush)FindResource("BrushBorder");
            _carteSelectionnee.Background = (Brush)FindResource("BrushSurface");
        }

        carte.BorderBrush = (Brush)FindResource("BrushPrimary");
        carte.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
        _carteSelectionnee = carte;

        _etat.Service = service;
        BtnSuivant.IsEnabled = true;
        TxtErreur.Text = "";
    }

    private void BtnPrecedent_Click(object sender, RoutedEventArgs e) => _allerPrecedent();

    private void BtnSuivant_Click(object sender, RoutedEventArgs e)
    {
        if (_etat.Service is null)
        {
            TxtErreur.Text = "Sélectionne un service pour continuer.";
            return;
        }

        // Le service change eventuellement la contrainte de delai :
        // on reinitialise les choix des etapes suivantes pour eviter
        // une incoherence (ex: "Aujourd'hui" choisi puis on revient
        // choisir un service qui l'interdit).
        _etat.RdvAujourdHui = false;
        _etat.DateChoisie = null;
        _etat.Creneau = null;
        _etat.Medecin = null;

        _allerSuivant();
    }
}
