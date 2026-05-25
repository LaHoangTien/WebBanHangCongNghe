using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Models
{
    public class StatisticsViewModel
    {
        public double TotalRevenue { get; set; }
        public List<Order> Orders { get; set; }
        public List<ProductWithQuantitySold> TopSellingProducts { get; set; }
        public List<MonthlyRevenue> MonthlyRevenues { get; set; }
        public int SelectedYear { get; set; }
        public decimal YearRevenue { get; set; }
    }

    public class ProductWithQuantitySold
    {
        public Product Product { get; set; }
        public int TotalQuantitySold { get; set; }
    }
    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal MonthRevenue { get; set; }
    }
    public class ProductIndexViewModel
    {
        public List<Product> AllProducts { get; set; }
        public List<ProductWithQuantitySold> TopSellingProducts { get; set; }
    }
}