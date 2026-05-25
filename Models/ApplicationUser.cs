using Microsoft.AspNetCore.Identity;

namespace WebBanHang.Models
{
    public class ApplicationUser: IdentityUser
    {
        public required string FullName { get; set; }
        public required string Address { get; set; }
		public ICollection<Wishlist> Wishlists { get; set; }

	}
}
