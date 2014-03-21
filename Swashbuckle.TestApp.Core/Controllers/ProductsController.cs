using System;
using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Core.Models;

namespace Swashbuckle.TestApp.Core.Controllers
{
    public class ProductsController : ApiController
    {
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
