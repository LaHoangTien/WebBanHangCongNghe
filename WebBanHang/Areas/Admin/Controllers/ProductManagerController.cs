using FuzzySharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using WebBanHang.DataAccess;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Utilitys;
using FuzzySharp;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductManagerController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductManagerController(IProductRepository productRepository, ICategoryRepository
        categoryRepository, IBrandRepository brandRepository, ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            var products = await _productRepository.GetAllAsync();

            const int pageSize = 9;
            if (pg < 1)
                pg = 1;
            int recsCount = products.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recskip = (pg - 1) * pageSize;
            var data = products.Skip(recskip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;

            return View(data);
        }

        public async Task<IActionResult> SearchProduct(string searchString, int pg = 1)
        {
            const int pageSize = 9;

            // Tính toán tổng số sản phẩm sau khi áp dụng điều kiện tìm kiếm
            var totalItems = _context.Products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString)).Count();



            // Lấy danh sách sản phẩm theo trang hiện tại và kích thước trang
            var products = _context.Products
                .Where(p => p.Name.Contains(searchString))
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Tạo đối tượng Pager
            var pager = new Pager(totalItems, pg, pageSize);

            // Truyền dữ liệu và pager đến view
            ViewBag.SearchString = searchString;
            ViewBag.Pager = pager;
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
                foreach (var imageFile in images)
                {
                    var imagePath = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, "images", imagePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    var productImage = new ProductImage
                    {
                        Url = "/images/" + imagePath
                    };

                    product.Images.Add(productImage);
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
                existingProduct.CostPrice = product.CostPrice;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Images = product.Images;
                existingProduct.Stock = product.Stock;
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
