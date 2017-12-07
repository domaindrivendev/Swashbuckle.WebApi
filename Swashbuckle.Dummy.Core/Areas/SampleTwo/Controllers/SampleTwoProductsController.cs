using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Swashbuckle.Dummy.Areas.SampleTwo.Controllers
{
    [System.Web.Http.RoutePrefix("sampletwo/api")]
    public class SampleTwoProductsController : ApiController
    {
        readonly Random _random = new Random();

        public int Create(Product product) => _random.Next();

        [System.Web.Http.HttpPut]
        [Route("")]
        public string Update(int id, Product product)
        {
            return $"product {id} updated";
        }

        public void Delete(int id)
        {

        }

        [Route("")]
        public Product Get(int id)
        {
            return new Product
            {
                Id = id,
                Name = $"Product {id}"
            };
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
