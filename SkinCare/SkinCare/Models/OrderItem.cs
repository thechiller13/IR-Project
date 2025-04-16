using System.ComponentModel.DataAnnotations;

namespace SkinCare.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
    }
}
