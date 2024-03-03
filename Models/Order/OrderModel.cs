using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Order
{
    public enum EnumStatus
    {
        Pending,
        Processing,
        Completed
    }
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string FullName { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string? OrderNote {  get; set; }
        public decimal SubTotal {  get; set; }
        public decimal Total { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public DateTime? DateCreated { get; set; }

        public EnumStatus Status { get; set; }
    }
}
