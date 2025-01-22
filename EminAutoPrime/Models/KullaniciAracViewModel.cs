namespace EminAutoPrime.Models
{
    public class KullaniciAracViewModel
    {
        public int AracId { get; set; } 
        public string Plaka { get; set; } 
        public int Yil { get; set; } 
        public string MarkaAdi { get; set; } 
        public string ModelAdi { get; set; } 
        public string ServisDurumu { get; set; } 
        public string ServisAciklama { get; set; }
        public List<ServisDetayViewModel> ServisIslemleri { get; set; }
    }
}
