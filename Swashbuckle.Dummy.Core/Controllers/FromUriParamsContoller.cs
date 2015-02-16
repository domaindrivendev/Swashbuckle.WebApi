using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class FromUriParamsController : ApiController
    {
        [HttpHead]
        public bool SupportsCurrencies([FromUri]IEnumerable<string> currencies)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public decimal CalculateTax([FromUri]Transaction transaction, [FromUri]BillingInfo billingInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class Transaction
    {
        [Required]
        public string Currency { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }

    public class BillingInfo
    {
        [Required]
        public Address BillTo { get; set; }

        public Address ShipTo { get; set; }
    }

    public class Address
    {
        [Required]
        public string Country { get; set; }

        public string City { get; set; }
    }
}