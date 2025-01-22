namespace EminAutoPrime.Models
{
    public class YeniIslemViewModel
    {
        public int ServisId { get; set; }
        public int? ParcaId { get; set; }
        public int IslemTipId { get; set; }
        public string IslemAciklama { get; set; }
        public decimal IslemMaliyeti { get; set; }
        public DateTime IslemTarihi { get; set; }
    }
}
