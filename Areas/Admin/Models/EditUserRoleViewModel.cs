namespace WebBanHang.Areas.Admin.Models
{
	public class EditUserRoleViewModel
	{
        public EditUserRoleViewModel()
        {
            CurrentRoles = new List<string>();
            AllRoles = new List<string>();
            SelectedRoles = new List<string>();
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public IList<string> CurrentRoles { get; set; }
        public IList<string> AllRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }
}
