using System;
using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class ProductsController : ApiController
    {
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
