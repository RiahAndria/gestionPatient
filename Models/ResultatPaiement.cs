namespace Patients.Models
{
    public class ResultatPaiement
    {
        public bool Succes { get; set; }
        public string? MessageErreur { get; set; }
        public bool PaiementComplet { get; set; }
        public decimal MontantRestant { get; set; }
    }
}