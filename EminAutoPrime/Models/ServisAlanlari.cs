namespace EminAutoPrime.Models
{
    public class ServisAlanlari
    {
        public int ServisAlaniId { get; set; }
        public string ServisAdi { get; set; }

        public ICollection<ServisIslemleri> ServisIslemleri { get; set; }
        public ICollection<Randevular> Randevular { get; set; }
    }
}
