# Gestion Médicale

Application de bureau Windows destinée à la gestion d'un cabinet ou d'un établissement médical. Elle permet de centraliser les patients, les dossiers médicaux, les médecins, les rendez-vous, les consultations, les paiements et les notifications dans une interface WPF.

> Projet réalisé en C# avec .NET 10, WPF, PostgreSQL et Dapper.

## Sommaire

- [Fonctionnalités](#fonctionnalités)
- [Technologies](#technologies)
- [Prérequis](#prérequis)
- [Installation](#installation)
- [Configuration](#configuration)
- [Base de données](#base-de-données)
- [Lancement](#lancement)
- [Utilisation](#utilisation)
- [Architecture du projet](#architecture-du-projet)
- [Modèle de données](#modèle-de-données)
- [Développement](#développement)
- [Dépannage](#dépannage)
- [Sécurité et limites](#sécurité-et-limites)
- [Licence](#licence)

## Fonctionnalités

### Patients et dossiers médicaux

- Création, recherche, consultation, modification et suppression de patients.
- Génération et affichage du numéro de dossier patient.
- Gestion des informations personnelles : identité, date de naissance, genre, adresse, téléphone, e-mail et assurance.
- Consultation du dossier médical : poids, taille, groupe sanguin, allergies et antécédents.
- Consultation de l'historique des consultations d'un patient.
- Accès aux rendez-vous associés au patient.

### Médecins et disponibilités

- Ajout, consultation, modification et suppression de médecins.
- Gestion de la fonction médicale, du statut et du taux horaire.
- Ajout de fonctions médicales personnalisées.
- Gestion des créneaux et des disponibilités par médecin et par date.
- Affichage du planning hebdomadaire et des créneaux réservés ou disponibles.

### Rendez-vous

- Consultation de la liste des rendez-vous.
- Recherche et filtrage par statut : planifié, terminé ou annulé.
- Création d'un rendez-vous avec un assistant en plusieurs étapes :
  1. sélection du patient ;
  2. choix du service médical ;
  3. choix du type de rendez-vous ;
  4. sélection du médecin et du créneau ;
  5. récapitulatif ;
  6. choix du règlement ;
  7. confirmation.
- Consultation du détail d'un rendez-vous.
- Reprogrammation d'un rendez-vous.
- Changement de statut et ajout d'un motif d'annulation.
- Suppression d'un rendez-vous.

### Consultations et ordonnances

- Enregistrement d'une consultation à partir d'un rendez-vous terminé.
- Saisie du diagnostic et des notes médicales.
- Mise à jour du dossier médical au cours de l'enregistrement.
- Ajout d'une ordonnance avec traitement, durée et diagnostic.
- Consultation de l'historique des consultations.

### Paiements et facturation

- Enregistrement des paiements liés à un rendez-vous ou à une consultation.
- Prise en charge des paiements complets et des acomptes.
- Modes de paiement disponibles dans l'interface : espèces, Mobile Money, chèque et carte bancaire.
- Suivi des paiements réglés, incomplets, rejetés ou restant à facturer.
- Règlement d'un solde restant dû.
- Génération et affichage d'une facture.
- Gestion du statut de facturation.

### Notifications et rappels

- Affichage des notifications de réservation et de paiement.
- Badge indiquant le nombre de notifications non lues.
- Marquage des notifications comme lues.
- Génération de rappels liés aux rendez-vous.
- Génération de relances pour les paiements incomplets.

## Technologies

| Composant | Technologie |
| --- | --- |
| Langage | C# |
| Framework | .NET 10 |
| Interface | WPF |
| Base de données | PostgreSQL |
| Accès aux données | Dapper et Npgsql |
| Design | MaterialDesignThemes |
| Configuration | Microsoft.Extensions.Configuration.Json |
| SDK requis | .NET SDK `10.0.301` ou version compatible selon `global.json` |

L'application est une application Windows (`net10.0-windows`) et n'est pas prévue pour Linux ou macOS sans adaptation de l'interface WPF.

## Prérequis

Avant l'installation, disposer de :

- Windows ;
- le .NET SDK 10 ;
- PostgreSQL, démarré comme service local ou accessible sur le réseau ;
- un compte PostgreSQL capable de créer la base et les tables ;
- un terminal PowerShell ou l'invite de commandes ;
- éventuellement Visual Studio ou Visual Studio Code avec les outils C#/.NET.

Vérifier le SDK installé :

```powershell
dotnet --version
```

La version attendue est `10.0.301` ou une version plus récente acceptée par la règle `latestFeature` de `global.json`.

## Installation

### 1. Récupérer le projet

```powershell
git clone <URL_DU_DEPOT>
cd Patients
```

Si le projet est déjà présent localement, se placer directement dans le dossier qui contient `Patients.csproj`.

### 2. Restaurer les dépendances

```powershell
dotnet restore
```

### 3. Créer la base PostgreSQL

Créer une base vide nommée `gestion_patient_db`, ou utiliser un autre nom puis le reporter dans la chaîne de connexion :

```sql
CREATE DATABASE gestion_patient_db;
```

La commande peut être exécutée avec `psql` :

```powershell
createdb -U postgres gestion_patient_db
```

### 4. Initialiser le schéma

Depuis le dossier du projet, exécuter le script principal :

```powershell
psql -U postgres -d gestion_patient_db -f database/schema.sql
```

Appliquer ensuite les migrations dans l'ordre :

```powershell
psql -U postgres -d gestion_patient_db -f database/migrations/001_fix_doublons_fonction.sql
psql -U postgres -d gestion_patient_db -f database/migrations/002_ajout_type_notif_est_facture.sql
```

### 5. Charger les données de démonstration, si nécessaire

Le script de seed vide les tables concernées avant d'insérer les données de test. Il ne doit donc pas être exécuté sur une base contenant des données à conserver.

```powershell
psql -U postgres -d gestion_patient_db -f database/seed/donnees_test.sql
```

## Configuration

L'application lit la chaîne de connexion `ConnectionStrings:DefaultConnection` dans `appsettings.json`.

Créer ou adapter ce fichier à partir de l'exemple :

```powershell
Copy-Item appsettings.json.example appsettings.json
```

Puis renseigner les paramètres de votre serveur PostgreSQL :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gestion_patient_db;Username=postgres;Password=VOTRE_MOT_DE_PASSE"
  }
}
```

Paramètres principaux :

| Paramètre | Description | Valeur courante |
| --- | --- | --- |
| `Host` | Adresse du serveur PostgreSQL | `localhost` |
| `Port` | Port PostgreSQL | `5432` |
| `Database` | Nom de la base utilisée par l'application | `gestion_patient_db` |
| `Username` | Utilisateur PostgreSQL | `postgres` |
| `Password` | Mot de passe de cet utilisateur | à renseigner |

Ne jamais publier un vrai mot de passe dans le dépôt. Le fichier `appsettings.json` présent localement doit rester réservé à l'environnement de développement et être remplacé par une configuration propre dans chaque environnement.

## Lancement

### Depuis le terminal

```powershell
dotnet run
```

### Depuis Visual Studio Code

1. Ouvrir le dossier qui contient `Patients.csproj`.
2. Vérifier que le SDK .NET 10 est sélectionné.
3. Vérifier que PostgreSQL est démarré et que `appsettings.json` est correctement configuré.
4. Lancer le projet avec `dotnet run` ou la commande de débogage .NET.

### Compiler sans lancer

```powershell
dotnet build
```

Le fichier projet produit une application Windows (`WinExe`). Les fichiers générés se trouvent dans `bin/` et `obj/`; ils ne doivent pas être versionnés.

## Utilisation

Après le démarrage, la fenêtre **Gestion Médicale** affiche une navigation latérale. Les vues principales sont :

- **Patients** : formulaire de création et tableau de recherche des patients ;
- **Corps médical** : gestion des médecins, fonctions et disponibilités ;
- **Consultations** : saisie et consultation des consultations ;
- **Planning & Rendez-vous** : recherche, création et suivi des rendez-vous ;
- **Paiements** : paiements incomplets, règlement des soldes et factures ;
- **Notifications** : rappels de rendez-vous et relances de paiement.

Les actions de suppression et de changement de statut demandent généralement une confirmation. Les erreurs non gérées sont affichées dans une boîte de dialogue par l'application.

## Architecture du projet

```text
Patients/
├── App.cs                         # Initialisation WPF et gestion globale des erreurs
├── App.xaml                       # Ressources de l'application
├── MainWindow.xaml/.cs            # Fenêtre principale et navigation
├── Patients.csproj                # Cible .NET, packages et ressources
├── appsettings.json               # Configuration locale, non destinée au partage
├── appsettings.json.example       # Exemple de configuration
├── global.json                    # Version du SDK .NET attendue
├── Assets/                        # Ressources graphiques, dont l'arrière-plan
├── Helpers/                       # Génération d'identifiants et helpers métier
├── Models/                        # Modèles de données et objets d'affichage
├── Services/                      # Accès aux données et logique métier
│   ├── Patient/
│   ├── Medecin/
│   ├── RendezVous/
│   ├── Consultation/
│   ├── Paiement/
│   ├── Disponibilite/
│   └── Rappel/
├── Views/                         # Vues WPF, formulaires et fenêtres secondaires
├── Themes/                        # Styles et ressources visuelles
└── database/
    ├── schema.sql                 # Création du schéma PostgreSQL
    ├── migrations/                # Évolutions du schéma
    └── seed/                      # Données de démonstration
```

L'accès aux données est réalisé directement par les services avec Dapper et Npgsql. Les fichiers partiels (`*.Creation.cs`, `*.Lecture.cs`, `*.Modification.cs`, etc.) regroupent les opérations d'un même service par responsabilité.

## Modèle de données

Le schéma PostgreSQL contient notamment les tables suivantes :

- `personne` : informations communes d'identité ;
- `patient` : informations administratives et numéro de dossier ;
- `dossier_medical` : informations médicales du patient ;
- `medecin` : statut, fonction, numéro d'ordre et taux horaire ;
- `fonction` : fonctions ou spécialités médicales ;
- `disponibilite` et `temps` : disponibilités et créneaux des médecins ;
- `rendez_vous` : planification et statut des rendez-vous ;
- `consultation` : diagnostic et notes médicales ;
- `ordonance` : traitements prescrits ;
- `paiement` : acomptes, paiements normaux et facturation ;
- `notification` : rappels et relances à afficher dans l'application.

Les statuts de rendez-vous utilisés par les données de démonstration sont `PLANIFIE`, `TERMINE` et `ANNULE`. Les paiements distinguent notamment les types `ACOMPTE` et `NORMAL`.

## Développement

### Restaurer, compiler et vérifier rapidement

```powershell
dotnet restore
dotnet build --no-restore
```

Le dépôt ne contient actuellement pas de projet de tests automatisés identifié. Les vérifications fonctionnelles doivent donc être effectuées dans l'application avec une base de développement dédiée.

### Modifier le schéma

Pour toute évolution de la base :

1. modifier le script nécessaire ou ajouter une nouvelle migration numérotée ;
2. tester la migration sur une copie de la base ;
3. mettre à jour le seed si les données de démonstration sont concernées ;
4. documenter l'ordre d'exécution dans ce README si nécessaire.

Éviter de modifier directement une base partagée sans migration traçable.

## Dépannage

### L'application ne démarre pas

- Vérifier que le SDK .NET 10 est installé avec `dotnet --version`.
- Exécuter `dotnet restore`, puis `dotnet build`.
- Vérifier la présence de `appsettings.json` à côté du projet.
- Vérifier que le fichier JSON est valide et que la clé `DefaultConnection` existe.

### Connexion PostgreSQL refusée

- Vérifier que le service PostgreSQL est démarré.
- Vérifier `Host`, `Port`, `Username`, `Password` et `Database`.
- Tester la connexion indépendamment :

```powershell
psql -U postgres -d gestion_patient_db
```

- Vérifier que le schéma a été exécuté dans la bonne base.

### Tables ou colonnes manquantes

Réinitialiser uniquement une base de développement, puis exécuter dans cet ordre :

```powershell
psql -U postgres -d gestion_patient_db -f database/schema.sql
psql -U postgres -d gestion_patient_db -f database/migrations/001_fix_doublons_fonction.sql
psql -U postgres -d gestion_patient_db -f database/migrations/002_ajout_type_notif_est_facture.sql
psql -U postgres -d gestion_patient_db -f database/seed/donnees_test.sql
```

### Données de test absentes

Le seed est facultatif. Le relancer recharge les données de démonstration, mais supprime d'abord les données des tables concernées avec `TRUNCATE ... CASCADE`.

## Sécurité et limites

- L'application manipule des données médicales et personnelles sensibles : utiliser exclusivement des bases et comptes adaptés à l'environnement concerné.
- Les identifiants PostgreSQL ne doivent pas être stockés en clair dans un dépôt partagé.
- Le projet ne présente pas de système d'authentification ou de gestion de rôles visible dans l'interface actuelle.
- La base de données doit être sauvegardée avant toute migration ou exécution du seed.
- WPF limite la cible principale à Windows.
- Les dates présentes dans `database/seed/donnees_test.sql` sont des données de démonstration et ne constituent pas des données métier à conserver.

## Licence

Ce projet est distribué sous licence **MIT**. Consultez le fichier [LICENCE](LICENCE) pour connaître les conditions d'utilisation, de modification et de redistribution.
