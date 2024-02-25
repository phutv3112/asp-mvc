using AppMVC.Areas.Product.Models;
using AppMVC.Helpers;
using AppMVC.Models;
using AppMVC.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService)
        {
            _context = context;
            _logger = logger;
            _cartService = cartService;
        }

        // GET: ViewPostController
        [Route("/product/{categoryslug?}")]
        public ActionResult Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;
            CategoryProduct category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.CategoryProducts.Where(c => c.Slug == categoryslug)
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefault();
                if (category == null)
                {
                    return NotFound();
                }
            }
            var products = _context.Products.Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategories)
                                    .ThenInclude(p => p.Category)
                                    .AsQueryable();
            products.OrderByDescending(p => p.DateUpdated);

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildrenCategoryIDs(null, ids);
                ids.Add(category.Id);

                products = products.Where(p => p.ProductCategories.Where(pc => ids.Contains(pc.CategoryId)).Any());
            }

            int totalProduct = products.Count();
            if (pageSize <= 0) pageSize = 6;
            int countPage = (int)Math.Ceiling((double)totalProduct / pageSize);

            if (currentPage > countPage) currentPage = countPage;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countPages = countPage,
                currentPage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pageSize
                })
            };

            var productInPage = products.Skip((currentPage - 1) * pageSize)
                           .Take(pageSize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalProduct = totalProduct;

            ViewBag.category = category;
            return View(productInPage.ToList());
        }
        [Route("/product/{productslug}.html")]
        public IActionResult Detail(string productslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var product = _context.Products.Where(p => p.Slug == productslug)
                            .Include(p => p.Photos)
                            .Include(p => p.Author)
                            .Include(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }

            CategoryProduct category = product.ProductCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherProducts = _context.Products.Where(p => p.ProductCategories.Any(c => c.Category.Id == category.Id))
                                    .Where(p => p.ProductId != product.ProductId)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .Take(5);
            ViewBag.otherProducts = otherProducts;
            return View(product);
        }
        private List<CategoryProduct> GetCategories()
        {
            var categories = _context.CategoryProducts.Include(c => c.CategoryChildren)
                                            .AsEnumerable()
                                            .Where(c => c.CategoryParent == null)
                                            .ToList();
            return categories;
        }
        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {

            var product = _context.Products
                .Where(p => p.ProductId == productid)
                .FirstOrDefault();
            if (product == null)
                return NotFound("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity++;
            }
            else
            {
                //  Thêm mới
                cart.Add(new CartItem() { quantity = 1, Product = product });
            }

            // Lưu cart vào Session
            _cartService.SaveCartSession(cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction(nameof(Cart));
        }
        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity = quantity;
            }
            _cartService.SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(c => c.Product.ProductId == productid);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }
            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }
        [Route("/checkout")]
        public IActionResult Checkout()
        {

            _cartService.ClearCart();
            return Content("Checkout done");
        }
    }
}
