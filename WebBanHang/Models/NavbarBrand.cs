using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAccess;
using WebBanHang.Repositories;

namespace WebBanHang.Models
{
    public class NavbarBrand: ViewComponent
    {
        private readonly ApplicationDbContext _context;


        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        public NavbarBrand(ApplicationDbContext context, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
        {
            _context = context;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }
        public IViewComponentResult Invoke()
        {

            return View(_context.Brands.ToList());
        }
    

    }
}
