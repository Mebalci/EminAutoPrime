using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class EminAutoArac
    {
        [Key]
        public int AracId { get; set; }

        [Required]
        public string Marka { get; set; }

        [Required]
        public string Model { get; set; }

        public int Yil { get; set; }

        public string Plaka { get; set; }

        public string SahipAdi { get; set; }
    }
}
