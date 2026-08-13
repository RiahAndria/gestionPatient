using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Patients.Views.RendezVous.Assistant;

// Fenetre hote de l'assistant "Nouveau rendez-vous" en 7 etapes (voir
// le cahier des charges du flux dans le README / la demande initiale).
//
// Remplace l'ancienne RendezVousFormView (formulaire a un seul ecran) :
// RendezVousListView.btnNouveau_Click ouvre desormais cette fenetre.
// L'ancien RendezVousFormView.xaml/.cs est conserve dans le depot (au
// cas ou il faille y revenir rapidement) mais n'est plus branche a
// aucun bouton.
public partial class NouveauRendezVousWindow : Window
{
    private const int NB_ETAPES = 7;

    private static readonly string[] _titres =
    {
        "Étape 1/7 — Sélection du patient",
        "Étape 2/7 — Choix du service médical",
        "Étape 3/7 — Type de rendez-vous",
        "Étape 4/7 — Créneau horaire et médecin",
        "Étape 5/7 — Récapitulatif",
        "Étape 6/7 — Règlement du paiement",
        "Étape 7/7 — Confirmation",
    };

    private readonly AssistantRendezVousState _etat = new();
    private int _etapeActuelle = 1;

    // Devient vrai uniquement si l'assistant a ete mene jusqu'au bout
    // (etape 7 atteinte) : utilise par l'appelant (RendezVousListView)
    // pour savoir s'il doit rafraichir sa grille.
    public bool RendezVousCree { get; private set; }

    public NouveauRendezVousWindow(string? patientIdPreselectionne = null)
    {
        InitializeComponent();
        _etat.PatientIdPreselectionne = patientIdPreselectionne;
        AfficherEtape(1);
    }

    private void AfficherEtape(int numero)
    {
        _etapeActuelle = numero;
        TxtTitreEtape.Text = _titres[numero - 1];
        MettreAJourBarreProgression(numero);

        ContenuEtape.Content = numero switch
        {
            1 => new Etape1SelectionPatientView(_etat, () => AfficherEtape(2)),
            2 => new Etape2ChoixServiceView(_etat, () => AfficherEtape(3), () => AfficherEtape(1)),
            3 => new Etape3TypeRdvView(_etat, () => AfficherEtape(4), () => AfficherEtape(2)),
            4 => new Etape4CreneauMedecinView(_etat, () => AfficherEtape(5), () => AfficherEtape(3)),
            5 => new Etape5RecapitulatifView(_etat, () => AfficherEtape(6), () => AfficherEtape(4)),
            6 => new Etape6PaiementView(_etat, () => AfficherEtape(7), () => AfficherEtape(5)),
            7 => new Etape7ConfirmationView(_etat, Terminer),
            _ => ContenuEtape.Content
        };
    }

    private void MettreAJourBarreProgression(int numero)
    {
        var barres = new[] { Barre1, Barre2, Barre3, Barre4, Barre5, Barre6, Barre7 };
        var actif = (Brush)FindResource("BrushPrimary");
        var inactif = (Brush)FindResource("BrushBorder");

        for (int i = 0; i < barres.Length; i++)
        {
            barres[i].Background = (i < numero) ? actif : inactif;
        }
    }

    // Appele par Etape7ConfirmationView quand l'utilisateur clique sur
    // "Retour à la liste des rendez-vous".
    private void Terminer()
    {
        RendezVousCree = true;
        Close();
    }
}
