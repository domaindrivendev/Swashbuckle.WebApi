using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.TestApp.Core.Models
{
    public class OrderItem
    {
        [Required]
        public int LineNo { get; set; }

        [Required]
        public string Product { get; set; }

        public ProductCategory Category { get; set; }

        public int Quantity { get; set; }
    }

    public enum ProductCategory
    {
        Category1,
        Category2,
        Category3
    }
}