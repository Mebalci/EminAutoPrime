namespace EminAutoPrime.Models
{
    public class ServisViewModel
    {
        public int ServisId { get; set; }
        public string AracBilgisi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? TahminiBitisTarihi { get; set; }
        public string Durum { get; set; }
        public string Aciklama { get; set; }
        public decimal? ToplamMaliyet { get; set; }
        public string? ServisAlani { get; set; }
    }
}
