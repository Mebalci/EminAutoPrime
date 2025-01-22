namespace EminAutoPrime.Models
{
    public class ParcaKategorileri
    {
        public int KategoriId { get; set; }
        public string KategoriAdi { get; set; }

        public ICollection<Parcalar> Parcalar { get; set; }
    }
}
