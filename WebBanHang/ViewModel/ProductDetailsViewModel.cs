using WebBanHang.Models;

namespace WebBanHang.ViewModel
{
	public class ProductDetailsViewModel
	{
		public Product Product { get; set; }
		public Brand Brand { get; set; }
		public Category Category { get; set; }
		public List<Comment> Comments { get; set; }
		public List<ProductReview> Reviews { get; set; }
		public double AverageRating { get; set; }
	}
}
