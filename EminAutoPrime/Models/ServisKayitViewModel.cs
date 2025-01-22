using Microsoft.AspNetCore.Mvc.Rendering;

namespace EminAutoPrime.Models
{
    public class ServisKayitViewModel
    {
        public int AracId { get; set; }
        public string KullaniciAdi { get; set; }   
        public string AracBilgisi { get; set; }   
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? TahminiBitisTarihi { get; set; }
        public int DurumId { get; set; }
        public string Aciklama { get; set; }
        public decimal ToplamMaliyet { get; set; }
        public List<SelectListItem> DurumListesi { get; set; }
        public List<SelectListItem> AracListesi { get; set; }
    }
}
