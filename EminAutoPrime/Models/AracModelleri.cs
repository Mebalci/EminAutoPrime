namespace EminAutoPrime.Models
{
    public class AracModelleri
    {
        public int ModelId { get; set; }
        public string ModelAdi { get; set; }
        public int MarkaId { get; set; }
        public AracMarkalari Marka { get; set; }
        public ICollection<Araclar> Araclar { get; set; }
    }
}
