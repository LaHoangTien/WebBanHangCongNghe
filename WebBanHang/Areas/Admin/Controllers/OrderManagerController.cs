using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DataAccess;
using WebBanHang.Migrations;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Utilitys;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderManagerController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
       
        public OrderManagerController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
       
        public IActionResult Index(int pg = 1)
        {

           
            var orders = _context.Orders
                .Include(o => o.OrderDetails).OrderByDescending(x => x.OrderDate).ToList();

            const int pageSize = 5;
            if (pg < 1)
                pg = 1;
            int recsCount = orders.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = orders.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }
        public async Task<IActionResult> SearchOrder(string searchString, int pageNumber = 1)
        {
            const int pageSize = 9;

            // Tính toán tổng số sản phẩm sau khi áp dụng điều kiện tìm kiếm
            var totalItems = _context.Orders.Where(p => p.Name.Contains(searchString) || p.PhoneNumber.Contains(searchString)).Count();



            // Lấy danh sách sản phẩm theo trang hiện tại và kích thước trang
            var orders = _context.Orders
                 .Include(o => o.OrderDetails)
                 .Where(p => p.Name.Contains(searchString))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Tạo đối tượng Pager
            var pager = new Pager(totalItems, pageNumber, pageSize);

            // Truyền dữ liệu và pager đến view
            ViewBag.SearchString = searchString;
            ViewBag.Pager = pager;
            return View(orders);
        }
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public async Task<IActionResult> EditOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> EditOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
