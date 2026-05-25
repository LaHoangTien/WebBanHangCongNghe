using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAccess;
using WebBanHang.Migrations;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CommentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       
        [Authorize]
        public async Task<IActionResult> Create(int productId, string content, int? parentCommentId)
        {

            var comment = new Comment
            {
                ProductID = productId,
                UserId = _userManager.GetUserId(User),
                UserName = _userManager.GetUserName(User),
                Content = content,
                CommentDate = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Home", new { id = productId });
        }

       
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Home", new { id = comment.ProductID });
        }
      
    }
}

