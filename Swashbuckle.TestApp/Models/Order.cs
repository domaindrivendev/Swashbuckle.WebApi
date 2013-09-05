using System.Collections.Generic;

namespace Swashbuckle.TestApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public decimal Total { get; set; }

        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}