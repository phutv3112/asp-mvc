using AppMVC.Models.Product;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Pkcs;

namespace AppMVC.Models.Order
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId {  get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
        public int OrderId {  get; set; }
        [ForeignKey("OrderId")]
        public OrderModel Order { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
