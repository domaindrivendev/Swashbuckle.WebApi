using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.TestApp.Models
{
    public class Order
    {
        [Required]
        public int Id { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Total { get; set; }

        public MyGenericType<OrderItem> GenericType1 { get; set; }

        public MyGenericType<ProductCategory> GenericType2 { get; set; }
    }
}