using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
		public List<ProductImage> Images { get; set; } = new List<ProductImage>();
		public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
		public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
		public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
		public ICollection<Visit> Visits { get; set; }
		public ICollection<Sale> Sales { get; set; }
        public decimal CostPrice { get; set; }
    }

}

