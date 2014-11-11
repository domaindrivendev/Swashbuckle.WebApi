using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ComplexTypesFromUriController : ApiController
    {
        [HttpGet]
        public decimal CalculateTax([FromUri]Address address, [FromUri]Transaction transaction)
        {
            throw new NotImplementedException();
        }
    }

    public class Address
    {
        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }
    }

    public class Transaction
    {
        public string Currency { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}