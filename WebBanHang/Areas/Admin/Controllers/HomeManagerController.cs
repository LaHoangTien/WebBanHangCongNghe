using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nest;
using WebBanHang.Areas.Admin.Models;
using WebBanHang.DataAccess;
using WebBanHang.Models;
using WebBanHang.Utilitys;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HomeManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index( int? year)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var orders = await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
            var totalRevenue = orders.Sum(m => m.Revenue);

            // Lọc đơn hàng theo năm được chỉ định hoặc năm hiện tại

            orders = orders.Where(o => o.OrderDate.Year == currentYear).ToList();
            // Tính tổng doanh thu cho năm hiện tại hoặc năm được chỉ định
            var filteredOrders = orders.Where(o => o.OrderDate.Year == currentYear).ToList();
            // Tính tổng doanh thu cho mỗi tháng trong năm
            var monthlyRevenue = new List<MonthlyRevenue>();
            decimal totalRevenueForYear = 0;

            for (int month = 1; month <= 12; month++)
            {
                var revenueForMonth = filteredOrders
                    .Where(o => o.OrderDate.Month == month)
                    .Sum(o => o.Revenue);

                if (revenueForMonth > 0)
                {
                    monthlyRevenue.Add(new MonthlyRevenue
                    {
                        Year = currentYear,
                        Month = month,
                        MonthRevenue = revenueForMonth
                    });
                }

                totalRevenueForYear += revenueForMonth;
            }

            var topProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new ProductWithQuantitySold
                {
                    Product = g.First().Product, // Lấy thông tin của sản phẩm
                    TotalQuantitySold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(10)
                .ToListAsync();

            var viewModel = new StatisticsViewModel
            {
                TotalRevenue = (double)totalRevenue,
                Orders = orders,
                TopSellingProducts = topProducts,
                MonthlyRevenues = monthlyRevenue,
                SelectedYear = currentYear,
                YearRevenue = totalRevenueForYear
            };

            return View(viewModel);
        }
      
    }
}
