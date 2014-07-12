using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class AnimalsController : ApiController
    {
		public IEnumerable<Animal> GetAll()
        {
            throw new NotImplementedException();
        }
    }
	
	public class Animal
    {
        public int Id { get; set; }

        public string Type { get; set; }
    }
}
