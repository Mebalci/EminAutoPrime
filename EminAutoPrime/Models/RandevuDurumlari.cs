namespace EminAutoPrime.Models
{
    public class RandevuDurumlari
    {
        public int DurumId { get; set; }
        public string DurumAdi { get; set; }

        public ICollection<Randevular> Randevular { get; set; }
    }
}
