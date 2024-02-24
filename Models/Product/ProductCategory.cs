using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    public class ProductCategory
    {
        public int ProductId { set; get; }
        public int CategoryId { set; get; }
        [ForeignKey("ProductId")]
        public ProductModel Product { set; get; }
        [ForeignKey("CategoryId")]
        public CategoryProduct Category { set; get; }
    }
}