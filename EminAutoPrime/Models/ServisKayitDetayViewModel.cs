using System.Collections.Generic;

namespace EminAutoPrime.Models
{
    public class ServisKayitDetayViewModel
    {
        public int ServisId { get; set; }
        public string AracBilgisi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? TahminiBitisTarihi { get; set; }
        public string Durum { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public decimal? ToplamMaliyet { get; set; }
        public List<ParcaViewModel> Parcalar { get; set; } = new();
        public List<IslemViewModel> Islemler { get; set; } = new();
        public string ServisAlani { get; set; } = string.Empty;
        public int ServisAlaniId { get; set; }
        public string CalisanAdiSoyadi { get; set; } = string.Empty;
        public List<EskiServisKayitlariViewModel> EskiServisKayitlari { get; set; } = new();
       
    }
}
