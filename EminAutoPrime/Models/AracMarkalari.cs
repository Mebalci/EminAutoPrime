namespace EminAutoPrime.Models
{
    public class AracMarkalari
    {
        public int MarkaId { get; set; }
        public string MarkaAdi { get; set; }
        public ICollection<AracModelleri> Modeller { get; set; }
    }
}
