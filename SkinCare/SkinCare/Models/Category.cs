using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SkinCare.Models
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [StringLength(200)]
        public string Description { get; set; }
        
        public virtual ICollection<Product> Products { get; set; }
    }
}
