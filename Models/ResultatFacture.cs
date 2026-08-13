namespace Patients.Models
{
    public class ResultatFacture
    {
        public bool FactureCreee { get; set; }
        public decimal Montant { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}