using System.Collections.Generic;

namespace Swashbuckle.TestApp.Models
{
    /// <summary>
    /// A customer
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// The Customer ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Customer's Associates
        /// </summary>
        public IEnumerable<Customer> Associates { get; set; }
    }
}