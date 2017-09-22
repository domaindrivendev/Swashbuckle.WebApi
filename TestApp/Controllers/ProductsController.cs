using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace TestApp.Controllers
{
    public class ProductsController : ApiController
    {
        public ProductList GetAll()
        {
            var productList = new ProductList
            {
                PageNo = 1,
                PageSize = 10,
            };

            productList.Add(new Product { Id = 1, Description = "Foo" });
            productList.Add(new Product { Id = 2, Description = "Bar" });

            return productList;
        }

        [HttpPost, Route("batch-update")]
        public void BatchUpdate(ProductList products)
        {
            throw new NotImplementedException();
        }

        //public Product GetByName(string name)
        //{
        //    throw new NotImplementedException();
        //}
    }

    [JsonObject]
    public class ProductList : List<Product>
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}