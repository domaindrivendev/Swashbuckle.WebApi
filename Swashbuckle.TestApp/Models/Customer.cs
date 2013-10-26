using System.Collections.Generic;

namespace Swashbuckle.TestApp.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public IEnumerable<Customer> Associates { get; set; }
    }
}