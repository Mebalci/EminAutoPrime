using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EminAutoPrime.Models
{
    [Authorize(Roles = "Admin")]
    public class Kampanya
    {
        [Key]
        public int KampanyaID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Kampanya Başlığı")]
        public string KampanyaBasligi { get; set; }

        [StringLength(500)]
        [Display(Name = "Kampanya Açıklaması")]
        public string KampanyaAciklamasi { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? BaslangicTarihi { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Display(Name = "Resim Verisi")]
        public byte[] GorselVerisi { get; set; } 
    }
}
