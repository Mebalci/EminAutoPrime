using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class AddVehicleViewModel
    {
        [Required(ErrorMessage = "Plaka alanı gereklidir.")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,3}[0-9]{2,4}$", ErrorMessage = "Geçerli bir plaka formatı giriniz (örn: 06ABC123).")]
        public string Plaka { get; set; }

        [Required(ErrorMessage = "Marka seçimi gereklidir.")]
        public int MarkaId { get; set; }

        [Required(ErrorMessage = "Model seçimi gereklidir.")]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Yıl bilgisi gereklidir.")]
        [Range(1900, 2100, ErrorMessage = "Geçerli bir yıl giriniz.")]
        public int Yil { get; set; }

        public IEnumerable<SelectListItem> MarkaListesi { get; set; }
    }
}
