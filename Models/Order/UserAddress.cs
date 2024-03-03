using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Order
{
    public class UserAddress
    {
        [Key]
        public int Id { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string FullName { get; set; }
        [Phone]
        public string Phone { get; set; }
        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}
