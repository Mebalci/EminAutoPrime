using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace EminAutoPrime.Models
{
    public class RandevuAlViewModel
    {
        public int AracId { get; set; }
        public string MarkaAdi { get; set; }
        public string ModelAdi { get; set; }
        public DateTime? RandevuTarihi { get; set; }
        public string Plaka { get; set; }       
        public IEnumerable<SelectListItem> ServisAlanlari { get; set; }
        public IEnumerable<SelectListItem> Saatler { get; set; }
        public int? SeciliServisAlaniId { get; set; }
        public string Aciklama { get; set; }

        public List<Kampanya> Kampanyalar { get; set; }
    }
}
