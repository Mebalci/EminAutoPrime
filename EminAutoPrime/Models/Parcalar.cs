namespace EminAutoPrime.Models
{
    public class Parcalar
    {
        public int ParcaId { get; set; }
        public string ParcaAdi { get; set; }
        public decimal Fiyat { get; set; }
        public int StokMiktari { get; set; }
        public int KategoriId { get; set; }
        public ParcaKategorileri Kategori { get; set; }

        public ICollection<ServisIslemleri> ServisIslemleri { get; set; }
    }
}
