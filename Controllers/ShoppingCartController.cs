
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using WebBanHang.DataAccess;
using WebBanHang.Migrations;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Services;
using WebBanHang.Utilitys;

namespace WebBanHang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
       private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayservice;
        public ShoppingCartController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IVnPayService vnPayservice)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _vnPayservice = vnPayservice;
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product =  GetProductFromDatabase(productId);
			var cart =
		   HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
		   ShoppingCart();
			var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

			if (product == null || product.Stock == 0)
			{
				return View("Error");
			}
			if (existingItem != null)
			{
				if (product.Stock < existingItem.Quantity + quantity)
				{
					return View("Error");
				}
				existingItem.Quantity += quantity;
			}
			else
			{
				if (product.Stock < quantity)
				{
					return View("Error");
				}
				var cartItem = new CartItem
				{
					ProductId = productId,
					Name = product.Name,
					Price = product.Price,
					Quantity = quantity,
					Product = product
				};
				cart.AddItem(cartItem);
			}

			HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
        public IActionResult PlusCart(int productId, int quantity)
        {
			// Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId

			var product = GetProductFromDatabase(productId);
			if (product.Stock == 0)
			{
				return View("Error");
			}

			var cartItem = new CartItem
			{
				ProductId = productId,
				Name = product.Name,
				Price = product.Price,
				Quantity = quantity + 1,
				Product = product

			};
			if (product.Stock < cartItem.Quantity)
			{
				return View("Error");
			}
			var cart =
			HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
			ShoppingCart();
			cart.AddItem(cartItem);
			HttpContext.Session.SetObjectAsJson("Cart", cart);
			return RedirectToAction("Index");
		}
        public IActionResult MinusCart(int productId, int quantity)
        {
			// Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
			cart.MinusItem(productId);
			HttpContext.Session.SetObjectAsJson("Cart", cart);
			return RedirectToAction("Index");
		}

           
           
        

        public IActionResult Index()
        {
            var cart =HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if(cart.Items.Count == 0)
            {
                return View("EmtyCart");
            }
            return View(cart);
        }
		public async Task<IActionResult> RemoveItem(int id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
			cart.RemoveItem(product.Id);
			HttpContext.Session.SetObjectAsJson("Cart", cart);
			return Redirect(Request.Headers["Referer"].ToString());
		}
		
		private Product GetProductFromDatabase(int productId)
        {
           var product= _context.Products.FirstOrDefault(p=>p.Id == productId);
            return product;
        }
      
        public IActionResult Checkout()
        {
            return View(new Order());
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, CartItem cartItem, string payment = "COD")
        {
			

				var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            decimal revenue = 0;
            if (payment == "Thanh toán qua ví VNpay")
			{

                var vnPayModel = new VnPaymentRequestModel
                {


                    Amount = cart.Items.Sum(i => (double)i.Price * i.Quantity),

                    CreatedDate = DateTime.Now,
                    Description = $"{order.Name} {order.PhoneNumber}",
                    FullName = order.Notes,
                    OrderId = new Random().Next(1000, 100000)
                };
                var user1 = await _userManager.GetUserAsync(User);
                order.UserId = user1.Id;
                order.PaymentExpression = payment;
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
					ProductName = i.Name,
					ProductImges = i.Product.ImageUrl,
					ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

                foreach (var item in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null && product.Stock >= item.Quantity)
                    {
                        product.Stock -= item.Quantity;
                        _context.Products.Update(product);
                        revenue += (product.Price - product.CostPrice) * item.Quantity;
                    }
                    else
                    {
                        // Nếu sản phẩm không đủ số lượng tồn kho, có thể xử lý lỗi ở đây.
                        ModelState.AddModelError("", "Không đủ số lượng sản phẩm tồn kho.");
                        return View("Error");
                    }
                }
                order.Revenue = revenue;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
			}
			if (cart == null || !cart.Items.Any())
                {
                    // Xử lý giỏ hàng trống...
                    return RedirectToAction("Index");
                }
           

                var user = await _userManager.GetUserAsync(User);
                order.UserId = user.Id;
                order.PaymentExpression = payment;
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    ProductName = i.Name,
                    ProductImges= i.Product.ImageUrl,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();
            foreach (var item in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && product.Stock >= item.Quantity)
                {
                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);
                    revenue += (product.Price - product.CostPrice) * item.Quantity;
                }
                else
                {
                    // Nếu sản phẩm không đủ số lượng tồn kho, có thể xử lý lỗi ở đây.
                    ModelState.AddModelError("", "Không đủ số lượng sản phẩm tồn kho.");
                    return View("Error");
                }
            }
            order.Revenue = revenue;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            

			return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng

        }
     

        [Authorize]
        public async Task<IActionResult> PaymentFail(int id)
        {

			return View();
			
        }
        [Authorize]
        public async Task<IActionResult> PaymentSuccess(Order order)
        {
          
            return View();
        }
      
       
        public async Task<IActionResult> PaymentCallBack(int id)
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
				
				TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.LoiThanhToan;
                    await _context.SaveChangesAsync();
                }

              
                return RedirectToAction("PaymentFail");
            }
            TempData["Message"] = $"Thanh toán VNPay thành công";
            var successfulOrder = await _context.Orders.FindAsync(id);
            if (successfulOrder != null)
            {
                successfulOrder.PaymentStatus = PaymentStatus.ThanhToanThanhCong;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PaymentSuccess");

        }
		
	}
}

