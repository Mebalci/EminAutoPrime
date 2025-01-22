using EminAutoPrime.Data;
using Microsoft.AspNetCore.Identity;

namespace EminAutoPrime.Models
{
    public class Araclar
    {
        public int AracId { get; set; }
        public string Plaka { get; set; }
        public int MarkaId { get; set; }
        public AracMarkalari Marka { get; set; }
        public int ModelId { get; set; }
        public AracModelleri Model { get; set; }
        public int Yil { get; set; }
        public string SahipId { get; set; }
        public AplicationUser Sahip { get; set; }

        public ICollection<ServisIslemleri> ServisIslemleri { get; set; }
    }
}
