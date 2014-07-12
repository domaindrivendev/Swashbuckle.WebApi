using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class PaymentsController : ApiController
    {
		public int Create(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
	
	public class Payment
    {
		[Required]
        public decimal Amount { get; set; }

		[Required]
        public string CardNumber { get; set; }

        public string Cvv { get; set; }
    }
}
