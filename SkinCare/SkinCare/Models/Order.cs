using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SkinCare.Models
{
    public class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public virtual IdentityUser User { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(100)]
        public string ShippingAddress { get; set; }
        
        [Required]
        [StringLength(50)]
        public string City { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Country { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
    
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
