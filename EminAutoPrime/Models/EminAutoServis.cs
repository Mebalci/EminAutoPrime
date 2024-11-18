using Microsoft.AspNetCore.Authorization;
using System;
using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    [Authorize(Roles = "Admin")]
    public class EminAutoServis
    {
        [Key]
        public int ServisId { get; set; }

        [Required]
        public int AracId { get; set; }

        public string YapilanIslemler { get; set; }

        public DateTime GirisTarihi { get; set; }

        public DateTime? CikisTarihi { get; set; }

        public bool Tamamlandi { get; set; }

        // İlişkili araç bilgisi
        public EminAutoArac Arac { get; set; }
    }
}
