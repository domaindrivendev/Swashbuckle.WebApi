using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ProductsController : ApiController
    {
        /// <summary>
        /// Create a new product 
        /// </summary>
        /// <remarks>Requires admin priveleges</remarks>
        /// <param name="product">New product details</param>
        /// <returns></returns>
        /// <response code="200">It's all good!</response>
        public int Create(Product product)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve product by unique Id
        /// </summary>
        /// <returns></returns>
        public Product GetById(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// List all products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Product> FindAll()
        {
            throw new NotImplementedException();
        }
        
        [HttpGet]
        public IEnumerable<Product> FindByType(ProductType type)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Describes a product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Uniquely identifies the product
        /// </summary>
        public int Id { get; set; }	

        public ProductType Type { get; set; }	
        public string Description { get; set; }	
        public decimal UnitPrice { get; set; }	
    }

    public enum ProductType
    {
        Book,
        Album
    }
}