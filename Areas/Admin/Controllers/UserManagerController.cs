using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Areas.Admin.Models;
using WebBanHang.DataAccess;
using WebBanHang.Models;
using WebBanHang.Utilitys;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserManagerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;
        public UserManagerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName, // Assuming UserName is the full name
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address, // Replace with actual address if available
                    Roles = roles
                });
            }
            const int pageSize = 9;
            if (pg < 1)
                pg = 1;
            int recsCount = model.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = model.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;

            return View(data);
        }
        
		public async Task<IActionResult> EditUserRole(string userId)
		{
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                CurrentRoles = await _userManager.GetRolesAsync(user),
                AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

		// Cập nhật role cho user
		[HttpPost]
		public async Task<IActionResult> EditUserRole(EditUserRoleViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user == null)
			{
				return NotFound();
			}

			var currentRoles = await _userManager.GetRolesAsync(user);
			var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
			var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

			var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
			if (!addResult.Succeeded)
			{
				ModelState.AddModelError("", "Failed to add roles");
				return View(model);
			}

			var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
			if (!removeResult.Succeeded)
			{
				ModelState.AddModelError("", "Failed to remove roles");
				return View(model);
			}

			return RedirectToAction("Index", "UserManager");
		}
	}
}
