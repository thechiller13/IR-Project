using System.ComponentModel.DataAnnotations;

namespace SkinCare.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        public string ImageUrl { get; set; }
        
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        
        public int Stock { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
