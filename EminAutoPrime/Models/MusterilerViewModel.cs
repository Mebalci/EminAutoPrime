using System.ComponentModel.DataAnnotations;

namespace EminAutoPrime.Models
{
    public class MusterilerViewModel
    {
        public string UserId { get; set; }
        public string Ad{ get; set; }
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
        public string SelectedRole { get; set; }
    }
}
