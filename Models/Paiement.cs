using System;

namespace Patients.Models;

public class Paiement
{
    public string NumeroPaiement { get; set; } = string.Empty; // Clé primaire
    public string NumeroConsultation { get; set; } = string.Empty; // FK
    public DateTime DatePaiement { get; set; } = DateTime.Now; // TIMESTAMP en SQL
    public decimal Montant { get; set; } // NUMERIC(10,2) en SQL
    public string ModePaiement { get; set; } = string.Empty;
    public bool Statut { get; set; } = false; // bool NOT NULL en SQL
}