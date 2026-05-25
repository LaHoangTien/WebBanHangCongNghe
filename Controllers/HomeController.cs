using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using WebBanHang.Services;
using WebBanHang.ViewModel;
using Nest;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using System.Drawing.Printing;
using WebBanHang.Migrations;
using WebBanHang.Areas.Admin.Models;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private ApplicationDbContext _context;
		private readonly ElasticSearchService _elasticSearchService;

		public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
		
		}
		
		public async Task<IActionResult> Index(string sortOrder,int pg = 1)
        {
            var topSellingProducts = await _context.OrderDetails
             .GroupBy(od => od.ProductId)
             .Select(g => new ProductWithQuantitySold
             {
                 Product = g.First().Product, // Lấy thông tin của sản phẩm
                 TotalQuantitySold = g.Sum(od => od.Quantity)
             })
             .OrderByDescending(x => x.TotalQuantitySold)
             .Take(12)
             .ToListAsync();

            // Lấy danh sách tất cả sản phẩm
            var products = await _productRepository.GetAllAsync();
            ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            switch (sortOrder)
            {

                case "price_desc":
                    products = products.Where(p => p.Price >= 0 && p.Price <= 10000000);
                    break;
                case "price_asc":
                    products = products.Where(p => p.Price >= 10000000 && p.Price <= 20000000);
                    break;
                case "price_dasc":
                    products = products.Where(p => p.Price >= 20000000 && p.Price <= 300000000);
                    break;
                case "price_esc":
                    products = products.Where(p => p.Price >= 30000000 && p.Price <= 500000000);
                    break;


            }
            // Phân trang cho tất cả sản phẩm
            const int pageSize = 12;
            if (pg < 1) pg = 1;

            int recsCount = products.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = products.Skip(recskip).Take(pager.PageSize).ToList();
            ViewBag.SortOrder = sortOrder;
            ViewBag.Pager = pager;

            // Tạo view model chứa cả hai danh sách sản phẩm
            var viewModel = new ProductIndexViewModel
            {
                AllProducts = data,
                TopSellingProducts = topSellingProducts
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Search(string searchString, int pageNumber = 1)
        {
            const int pageSize = 12;

            // Tính toán tổng số sản phẩm sau khi áp dụng điều kiện tìm kiếm
            var totalItems = _context.Products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString)).Count();

         

            // Lấy danh sách sản phẩm theo trang hiện tại và kích thước trang
            var products = _context.Products
                .Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Tạo đối tượng Pager
            var pager = new Pager(totalItems, pageNumber, pageSize);

            // Truyền dữ liệu và pager đến view
            ViewBag.SearchString = searchString;
            ViewBag.Pager = pager;
            return View(products);
        }


        public IActionResult GetProductByCate(string sortOrder,int id, int pg = 1)
        {

            ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            // Lấy dữ liệu sản phẩm từ cơ sở dữ liệu và lọc theo categoryId
            var products = _context.Products.Where(x => x.CategoryId == id);

            // Áp dụng sắp xếp theo tham số sortOrder
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.Where(p => p.Price >= 0 && p.Price <= 10000000);
                    break;
                case "price_asc":
                    products = products.Where(p => p.Price >= 10000000 && p.Price <= 20000000);
                    break;
                case "price_dasc":
                    products = products.Where(p => p.Price >= 20000000 && p.Price <= 300000000);
                    break;
                case "price_esc":
                    products = products.Where(p => p.Price >= 30000000 && p.Price <= 500000000);
                    break;
            }

            const int pageSize = 12;
			if (pg < 1)
				pg = 1;
			int recsCount = products.Count();
			var pager = new Pager(recsCount, pg, pageSize);
			int recskip = (pg - 1) * pageSize;
			var data = products.Skip(recskip).Take(pager.PageSize).ToList();
			this.ViewBag.Pager = pager;

			return View(data);
		}
        public IActionResult GetProductByBrand(string sortOrder,int id, int pg = 1)
		{
			ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
			var products = _context.Products.Where(x => x.BrandId == id);
			switch (sortOrder)
			{
				case "price_desc":
					products = products.Where(p => p.Price >= 0 && p.Price <= 10000000);
					break;
				case "price_asc":
					products = products.Where(p => p.Price >= 10000000 && p.Price <= 20000000);
					break;
				case "price_dasc":
					products = products.Where(p => p.Price >= 20000000 && p.Price <= 300000000);
					break;
				case "price_esc":
					products = products.Where(p => p.Price >= 30000000 && p.Price <= 500000000);
					break;
			}
			const int pageSize = 12;
            if (pg < 1)
                pg = 1;
            int recsCount = products.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = products.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;

            return View(data);
        }
        public async Task<IActionResult> Detail(int id)
        {
			var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
            .Include(p => p.Images)
			.Include(p => p.Reviews)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Replies)
			.FirstOrDefaultAsync(m => m.Id == id);

			if (product == null)
			{
				return NotFound();
			}
           
			var reviews = product.Reviews.ToList();
			double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
			var viewModel = new ProductDetailsViewModel
			{
				Product = product,
				Comments = product.Comments != null ? product.Comments.ToList() : new List<Comment>(),
				Reviews = reviews,
                Brand = product.Brand,
				AverageRating = averageRating
			};


			return View(viewModel);
		}
        
    }
}
