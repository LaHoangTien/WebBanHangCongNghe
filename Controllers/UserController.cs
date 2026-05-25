using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using WebBanHang.DataAccess;
using WebBanHang.Migrations;
using WebBanHang.Models;
using WebBanHang.Utilitys;

namespace WebBanHang.Controllers
{
    
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {

           
            return View(_userManager.Users.ToList());
        }
        public IActionResult edit(int id)
        {
            //if (_context.Roles.Any(r => r.Name == "User"))
            //{
            //    var store = new RoleStore<IdentityRole>(_context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = manager.Roles.First(r => r.Name == "User");

            //    role.Name = "NewNameForUser";
            //    manager.UpdateAsync(role);
            //}
            return View(User);
        }
        public IActionResult OrderHistory(int page = 1)
        {
            string userName = User.Identity.Name;

            // Truy vấn ID của người dùng từ cơ sở dữ liệu sử dụng tên người dùng
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user != null)
            {
                var userId = _userManager.GetUserId(User);
                // Lấy ID của người dùng

                int pageSize = 5; // Số mục trên mỗi trang

                // Truy vấn lịch sử đơn hàng của người dùng sử dụng ID người dùng
                var orders = _context.Orders.Where(o => o.UserId == userId).OrderByDescending(x => x.OrderDate).ToList();

                // Tạo đối tượng Pager để phân trang
                var pager = new Pager(orders.Count, page, pageSize);

                // Lọc và phân trang dữ liệu
                var pagedOrders = orders.Skip((pager.Currentpage - 1) * pager.PageSize).Take(pager.PageSize).ToList();

                ViewBag.Pager = pager;

                return View(pagedOrders);
            }

            // Xử lý trường hợp không tìm thấy người dùng
            return NotFound();
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

    }
}
