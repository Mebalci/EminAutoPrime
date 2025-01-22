using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Data
{
    public class AplicationUser : IdentityUser
    {
        public string KullaniciAdi { get; set; }
        public string KullaniciSoyadi { get; set; }
    }
}
