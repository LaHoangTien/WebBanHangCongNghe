using Microsoft.AspNetCore.Identity;

namespace WebBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public string PaymentExpression { get; set; }
        public decimal Revenue { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
    public enum PaymentStatus
    {
        ChuaThanhToan,
        ThanhToanThanhCong,
        LoiThanhToan
    }
}
