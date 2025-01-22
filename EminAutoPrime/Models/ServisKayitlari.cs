namespace EminAutoPrime.Models
{
    public class ServisKayitlari
    {
        public int ServisId { get; set; }
        public int AracId { get; set; }
        public Araclar Arac { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? TahminiBitisTarihi { get; set; }
        public int DurumId { get; set; }
        public ServisDurumlari Durum { get; set; }
        public string Aciklama { get; set; }
        public decimal ToplamMaliyet { get; set; }

        public ICollection<ServisIslemleri> ServisIslemleri { get; set; }
    }
}
