using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class ServisKayitDüzenleViewModel
    {
        [Required(ErrorMessage = "Kullanıcı ID alanı zorunludur.")]
        public string KullaniciId { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string KullaniciAdi { get; set; }

        [Required(ErrorMessage = "Kullanıcı soyadı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı soyadı en fazla 50 karakter olabilir.")]
        public string KullaniciSoyadi { get; set; }

        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefon numarası 10-11 haneli olmalıdır.")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Plaka alanı zorunludur.")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,3}[0-9]{2,4}$", ErrorMessage = "Geçerli bir plaka formatı giriniz (örn: 06ABC123).")]
        public string Plaka { get; set; }

        [Required(ErrorMessage = "Marka seçimi zorunludur.")]
        public int? MarkaId { get; set; }

        [Required(ErrorMessage = "Model seçimi zorunludur.")]
        public int? ModelId { get; set; }

        public List<SelectListItem> MarkaListesi { get; set; }
        public List<SelectListItem> ModelListesi { get; set; }
        public List<SelectListItem> Araçlar { get; set; }
    }
}
