namespace EminAutoPrime.Models
{
    public class ServisDurumlari
    {
        public int DurumId { get; set; }
        public string DurumAdi { get; set; }

        public ICollection<ServisKayitlari> ServisKayitlari { get; set; }
        public ICollection<Randevular> Randevular { get; set; }
    }
}
