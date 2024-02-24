using AppMVC.Models.Product;

namespace AppMVC.Areas.Product.Models
{
    public class CreateProductModel : ProductModel
    {
        public int[] CategoriesIDs { get; set; }
    }
}