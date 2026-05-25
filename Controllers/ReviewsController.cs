using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DataAccess;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
	public class ReviewsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		public async Task<IActionResult> AddReview(int productId, string userId, int rating, string review)
		{
			var product = await _context.Products.FindAsync(productId);
			if (product == null)
			{
				return NotFound();
			}
          
            var newReview = new ProductReview
			{
				ProductId = productId,
				UserId = userId,
				Rating = rating,
				Review = review,
				ReviewDate = DateTime.Now
			};

			_context.Reviews.Add(newReview);
			await _context.SaveChangesAsync();

			return RedirectToAction("Detail","Home", new { id = productId });
		}
		public async Task<double> CalculateAverageRating(int productId)
		{
			var ratings = await _context.Reviews
				.Where(r => r.ProductId == productId)
				.Select(r => r.Rating)
				.ToListAsync();

			double averageRating = ratings.Any() ? ratings.Average() : 0;

			return averageRating;
		}
	}
}
