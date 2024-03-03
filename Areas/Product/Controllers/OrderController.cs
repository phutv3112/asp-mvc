using AppMVC.Areas.Product.Models;
using AppMVC.Helpers;
using AppMVC.Models;
using AppMVC.Models.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    public class OrderController : Controller
    {
        private readonly CartService _cartService;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public OrderController(CartService cartService, UserManager<AppUser> userManager, AppDbContext context)
        {
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
        }
        [Route("/admin/order")]
        public async Task<IActionResult> Index([FromQuery(Name ="p")]int currentPage, int pageSize)
        {
            var listOrders = _context.Orders.OrderByDescending(o => o.DateCreated);
                
            var totalOrders = listOrders.Count();
            if (pageSize <= 0) pageSize = 10;
            int countPage = (int) Math.Ceiling((double) totalOrders / pageSize);

            if(currentPage > countPage) currentPage = countPage;
            if(currentPage <1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                currentPage = currentPage,
                countPages = countPage,
                generateUrl = (pageNumber)=> Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pageSize
                })
            };
            ViewBag.pagingModel = pagingModel;
            ViewBag.postIndex = (currentPage - 1) * pageSize;
            ViewBag.totalOrders = totalOrders;
            var orderInPage = await listOrders.Skip((currentPage - 1) * pageSize)
                        .Take(pageSize).ToListAsync();
                                        
            return View(orderInPage);
        }
        public async Task<IActionResult> Detail(int id) { 
            if(id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders
                            .Include(o => o.OrderItems)
                            .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(o=> o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            
            return View(order);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = _context.Orders
                           .Include(o => o.OrderItems)
                           .ThenInclude(i => i.Product)
                           .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
           
            return RedirectToAction("Checkout", "ViewProduct", new { area = "Product" });
            //return View("Checkout");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName, Country, Address, Phone, OrderNote")]CreateOrderModel model)
        {
            decimal total = _cartService.GetTotalAmount();

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Error: {error.ErrorMessage}");
                    }
                }

                return Content("model is invalid");
            }
            OrderModel order = new OrderModel()
            {
                FullName = model.FullName,
                Country  = model.Country,
                Address = model.Address,
                Phone = model.Phone,
                OrderNote = model.OrderNote,
                Total = total,
                SubTotal = total,
                UserId = _userManager.GetUserId(User),
                Status = EnumStatus.Pending,
                DateCreated = DateTime.Now,
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var cartItems = _cartService.GetCartItems();
            if(cartItems != null)
            {
                foreach (var item in cartItems)
                {
                    OrderItem orderItem = new OrderItem()
                    {
                        Quantity = item.quantity,
                        ProductId = item.Product.ProductId,
                        OrderId = order.Id,
                        Price = item.Product.Price
                    };
                    _context.OrdersItems.Add(orderItem);
                }
                await _context.SaveChangesAsync();
            }
            _cartService.ClearCart();
           
            return RedirectToAction(nameof(OrderSuccess));
        }
        public IActionResult OrderSuccess()
        {
            var qr = _context.Orders.OrderByDescending(o => o.DateCreated).Take(1);
            var categories = _context.CategoryProducts.Include(c => c.CategoryChildren)
                                                  .Include(c => c.CategoryParent)
                                                  .Where(c => c.ParentId == null);
           var order = qr.FirstOrDefault();
            ViewBag.categories = categories;
            return View(order);
        }
        public  IActionResult OrderDetail(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            var order =  _context.Orders.Where(o => o.Id == id)
                                       .Include(o => o.OrderItems)
                                       .ThenInclude(it => it.Product)
                                       .FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }

            
            var categories = _context.CategoryProducts.Include(c => c.CategoryChildren)
                                                  .Include(c => c.CategoryParent)
                                                  .Where(c => c.ParentId == null);

            ViewBag.categories = categories;
            return View(order);
        }
    }
}
