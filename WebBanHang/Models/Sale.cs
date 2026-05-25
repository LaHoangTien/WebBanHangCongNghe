namespace WebBanHang.Models
{
	public class Sale
	{
		public int SaleId { get; set; }
		public int ProductId { get; set; }
		public DateTime SaleDate { get; set; }
		public decimal Amount { get; set; }
		public Product Product { get; set; }
	}
}
