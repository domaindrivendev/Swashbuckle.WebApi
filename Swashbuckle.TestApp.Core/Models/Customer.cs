using System.Collections.Generic;

namespace Swashbuckle.TestApp.Core.Models
{
    /// <summary>
    /// Represents a registered customer
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Unique identifier for the customer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Lists all registered associates 
        /// </summary>
        public IEnumerable<Customer> Associates { get; set; }
    }
}