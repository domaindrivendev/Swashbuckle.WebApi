using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.Dummy.Models
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

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Lists all registered associates
        /// </summary>
        public IEnumerable<Customer> Associates { get; set; }
    }
}