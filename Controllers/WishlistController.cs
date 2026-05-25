using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DataAccess;
using WebBanHang.Migrations;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pg= 1)
        {
            var userId = _userManager.GetUserId(User);
            
            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();
            if (wishlistItems.Count == 0)
            {
                return View("Emty");
            }
            const int pageSize = 5;
            if (pg < 1)
                pg = 1;
            int recsCount = wishlistItems.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = wishlistItems.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> ToggleWishlist(int productId)
        {
			var user = await _userManager.GetUserAsync(User);

			var wishlistItem = await _context.Wishlists
				.FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

			if (wishlistItem != null)
			{
				// Nếu sản phẩm đã có trong wishlist, xóa nó
				_context.Wishlists.Remove(wishlistItem);
				await _context.SaveChangesAsync();
				return Json(new { success = true, added = false });
			}
			else
			{
				// Nếu sản phẩm chưa có trong wishlist, thêm nó vào
				var newWishlistItem = new Wishlist
				{
					UserId = user.Id,
					ProductId = productId
				};

				_context.Wishlists.Add(newWishlistItem);
				await _context.SaveChangesAsync();
				return Json(new { success = true, added = true });
			}
		}


		public async Task<IActionResult> RemoveFromWishlist(int id)
		{
			var wishlistItem = await _context.Wishlists.FindAsync(id);
			if (wishlistItem != null)
			{
				_context.Wishlists.Remove(wishlistItem);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}
		private int? GetCurrentUserId()
		{
			// Giả sử bạn đã thiết lập xác thực và có thể lấy ID của người dùng hiện tại từ claims principal.
			if (User.Identity.IsAuthenticated)
			{
				return int.Parse(User.FindFirst("UserId").Value);
			}

			return null;
		}
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> IsInWishlist(int productId)
		{
			var user = await _userManager.GetUserAsync(User);

			var isInWishlist = await _context.Wishlists
				.AnyAsync(w => w.UserId == user.Id && w.ProductId == productId);

			return Json(new { inWishlist = isInWishlist });
		}
	}
}
