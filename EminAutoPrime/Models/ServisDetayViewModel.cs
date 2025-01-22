using System.Collections.Generic;

namespace EminAutoPrime.Models
{
    public class ServisDetayViewModel
    {
        public int ServisId { get; set; }
        public int IslemId { get; set; }
        public string CalisanAdi { get; set; }
        public string ServisAlanAdi { get; set; }
        public string TakilanParcaAdi { get; set; }
        public string IslemTipi { get; set; }
        public string IslemAciklama { get; set; }
        public decimal IslemMaliyeti { get; set; }
        public DateTime IslemTarihi { get; set; }
      
    }


}
