using EminAutoPrime.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class KullaniciYorumlari
    {
        public int YorumId { get; set; }
        public string KullaniciId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir.")]
        public string YorumMetni { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Puan { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }

        public virtual AplicationUser Kullanici { get; set; }
    }
}
