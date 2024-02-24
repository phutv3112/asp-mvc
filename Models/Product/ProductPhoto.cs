using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    public class ProductPhoto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}