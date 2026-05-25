using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Brand
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
