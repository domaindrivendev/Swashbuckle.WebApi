using System.Collections.Generic;

namespace Swashbuckle.TestApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public decimal Total { get; set; }

        public MyGenericType<OrderItem> GenericType1 { get; set; }

        public MyGenericType<ProductCategory> GenericType2 { get; set; }
    }
}