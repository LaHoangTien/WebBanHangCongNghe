namespace WebBanHang.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public IList<string> Roles { get; set; } // Thêm thuộc tính Roles

        public UserViewModel()
        {
            Roles = new List<string>();
        }
    }
}
