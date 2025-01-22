using EminAutoPrime.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class Randevular
    {
        [Key]
        public int RandevuId { get; set; }
        public string MusteriId { get; set; }
        public AplicationUser Musteri { get; set; }
        public int ServisAlaniId { get; set; }
        public ServisAlanlari ServisAlani { get; set; }
        public int AracId { get; set; }
        public Araclar Arac { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public int DurumId { get; set; }
        public RandevuDurumlari Durum { get; set; }
        public DateTime TalepTarihi { get; set; }
        public string? Aciklama { get; set; }
    }
}
