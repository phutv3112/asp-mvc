using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Product.Models
{
    public class CreateOrderModel
    {
        //FullName, Country, Address,Phone, OrderNote
        public string FullName { get;set; }
        public string Country { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string Address { get; set; }
        public string? OrderNote { get;set;}

    }
}
