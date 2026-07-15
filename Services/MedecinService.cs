using System;

namespace Patients.Services;

public class MedecinService
{
    public bool EnregistrerMedecin(
        string nom, 
        string prenom, 
        DateTime dateNaissance, 
        string adresse, 
        string telephone, 
        string email, 
        string statut, 
        string fonction, 
        int tauxHoraire)
    {
        // TODO: Ajouter la logique d'enregistrement en base de données ou API plus tard.
        
        return true; 
    }
}