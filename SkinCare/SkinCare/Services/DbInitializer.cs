using Microsoft.EntityFrameworkCore;
using SkinCare.Data;
using SkinCare.Models;

namespace SkinCare.Services
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                // Check if there are any categories already
                if (!context.Set<Category>().Any())
                {
                    // Add default categories
                    var categories = new List<Category>
                    {
                        new Category { Name = "Cleansers", Description = "Products that clean your skin" },
                        new Category { Name = "Moisturizers", Description = "Products that hydrate your skin" },
                        new Category { Name = "Serums", Description = "Concentrated treatments for specific skin concerns" },
                        new Category { Name = "Masks", Description = "Intensive treatments for periodic use" },
                        new Category { Name = "Sunscreen", Description = "Products that protect your skin from UV rays" }
                    };

                    context.AddRange(categories);
                    context.SaveChanges();
                }
            }
        }
    }
}
