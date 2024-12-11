namespace EminAutoPrime.Models
{
    public class EminAutoAdminViewModel
    {
        public int TotalCars { get; set; } 
        public int TotalServices { get; set; } 
        public int ToplamKampanyalar { get; set; } 

        
        public List<Kampanya> Kampanyalar { get; set; }
    }
}
