using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.TestApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal Total { get; set; }

        [Required]
        public MyGenericType<OrderItem> GenericType1 { get; set; }

        [Required]
        public MyGenericType<ProductCategory> GenericType2 { get; set; }
    }
}