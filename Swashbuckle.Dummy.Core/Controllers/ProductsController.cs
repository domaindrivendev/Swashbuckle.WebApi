using System;
using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.Dummy.Models;

namespace Swashbuckle.Dummy.Controllers
{
    public class ProductsController : ApiController
    {
        [Route("products")]
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a list of products filtered by type
        /// </summary>
        /// <remarks>There are several different Product types</remarks>
        /// <returns></returns>
        [Route("products")]
        public IEnumerable<Product> GetByType(ProductType type, decimal? maxPrice = null)
        {
            throw new NotImplementedException();
        }

        [Route("products/{id}/suspend")]
        [HttpPut]
        [Obsolete]
        public void Suspend(int id)
        {
            throw new NotImplementedException();
        }
    }
}