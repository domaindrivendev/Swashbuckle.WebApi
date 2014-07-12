using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class KittensController : ApiController
    {
		public int Create(Kitten kitten)
        {
            throw new NotImplementedException();
        }
    }
	
	public class Kitten : Animal
    {
        public bool HasWhiskers { get; set; }
    }
}
