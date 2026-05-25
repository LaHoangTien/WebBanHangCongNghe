using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } // Số sao đánh giá
        public string Review { get; set; } // Nội dung đánh giá
        public DateTime ReviewDate { get; set; }
        public virtual Product Product { get; set; }
	
	}
}
