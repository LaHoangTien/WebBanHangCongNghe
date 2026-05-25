namespace WebBanHang.Models
{
	public class Visit
	{
		public int VisitId { get; set; }
		public int ProductId { get; set; }
		public DateTime VisitDate { get; set; }
		public Product Product { get; set; }
	}
}
