using AppMVC.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Components
{
    //AppMVC.Components.CategoryProductSidebar
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent
    {
        public class CategorySidebarData
        {
            public List<CategoryProduct> Categories { get; set; }
            public int level { get; set; }
            public string categoryslug { get; set; }
        }
        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}