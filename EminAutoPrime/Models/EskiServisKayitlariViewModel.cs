using System.Collections.Generic;

namespace EminAutoPrime.Models
{
    public class EskiServisKayitlariViewModel
    {
        public string KullaniciAdi { get; set; }
        public string AracBilgisi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public string Durum { get; set; }
        public string Aciklama { get; set; }
    }

}
