using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class PolymorphicTypesController : ApiController
    {
        public int Create(Elephant elephant)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Animal> GetAll()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Animal
    {
        public string Type { get; set; }
    }

    public class Mamal : Animal
    {
        public string HairColor { get; set; }
    }

    public class Elephant : Mamal
    {
        public int TrunkLength { get; set; }
    }
}