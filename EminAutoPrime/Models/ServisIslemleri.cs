using EminAutoPrime.Data;
using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Models
{
    public class ServisIslemleri
    {
        public int IslemId { get; set; }
        public int ServisId { get; set; }
        public ServisKayitlari Servis { get; set; }
        public string CalisanId { get; set; }
        public AplicationUser Calisan { get; set; }
        public int ServisAlaniId { get; set; }
        public ServisAlanlari ServisAlani { get; set; }
        public int? TakilanParcaId { get; set; }
        public Parcalar TakilanParca { get; set; }
        public int IslemTipId { get; set; }
        public ServisIslemTipleri IslemTip { get; set; }
        public string IslemAciklama { get; set; }
        public decimal IslemMaliyeti { get; set; }
        public DateTime IslemTarihi { get; set; }
        public int AracId { get; set; }
        public Araclar Arac { get; set; }
    }
}
