namespace EminAutoPrime.Models
{
    public class IslemViewModel
    {
        public int IslemId { get; set; }
        public string IslemAdi { get; set; }
        public string IslemAciklama { get; set; }
        public decimal? IslemMaliyeti { get; set; }
        public DateTime IslemTarihi { get; set; }
        public string CalisanAdi { get; set; }
        public string ServisAlani { get; set; }
    }
}
