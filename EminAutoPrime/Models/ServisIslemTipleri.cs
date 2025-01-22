namespace EminAutoPrime.Models
{
    public class ServisIslemTipleri
    {
        public int IslemTipId { get; set; }
        public string IslemAdi { get; set; }

        public ICollection<ServisIslemleri> ServisIslemleri { get; set; }
    }
}
