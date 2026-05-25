using WebBanHang.Models;
using WebBanHang.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebBanHang.Repositories;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authorization;
using WebBanHang.Utilitys;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DataAccess;
namespace WebBanHang.Controllers
{
    //[Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private ApplicationDbContext _context;
        public ProductController(IProductRepository productRepository, ICategoryRepository
        categoryRepository, IBrandRepository brandRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _context = context;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            var products = await _productRepository.GetAllAsync();

            const int pageSize = 20;
            if (pg < 1)
                pg = 1;
            int recsCount = products.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = products.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;

            return View(data);
        }
        public async Task<IActionResult> Search(string searchString, string sortOrder)
        {
            List<Product> products;
            if (searchString != "" && searchString != null)
            {
                products = _context.Products.Where(n => n.Name.Contains(searchString)).ToList();

            }
            else
            {
                products = _context.Products.ToList();
            }
            ViewData["NameSortParam"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            switch (sortOrder)
            {
               
                case "price_desc":
                    products = products.OrderBy(e => e.Price).ToList();
                    break;
                case "price_asc":
                    products = products.OrderByDescending(e => e.Price).ToList();
                    break;

              

            }
            return View(products);
        }
       
       
        // Hiển thị form thêm sản phẩm mới
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    product.ImageUrl = await SaveImage(imageUrl);
                }
				if (images != null)
				{
					product.Images = new List<ProductImage>();
					foreach (var file in images)
					{
						// Lưu các hình ảnh khác 
						var gallery = new ProductImage()
						{

							Url = await SaveImage(file)
						};
						product.Images.Add(gallery);
					}
				}

				await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(product);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != product.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.ImageUrl = product.ImageUrl;
				existingProduct.Images = product.Images;
				await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(product);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
