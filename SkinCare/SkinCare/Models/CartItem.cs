using System.ComponentModel.DataAnnotations;

namespace SkinCare.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        public string CartId { get; set; }
        
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
