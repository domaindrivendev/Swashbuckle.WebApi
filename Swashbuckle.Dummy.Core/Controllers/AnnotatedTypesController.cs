using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class AnnotatedTypesController : ApiController
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

        [Required, RegularExpression("^[3-6]?\\d{12,15}$")]
        public string CardNumber { get; set; }

        [Required, Range(1, 12)]
        public int ExpMonth { get; set; }

        [Required, Range(14, 99)]
        public int ExpYear { get; set; }

        public string Note { get; set; }
    }
}