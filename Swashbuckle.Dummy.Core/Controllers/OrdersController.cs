using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class OrdersController : ApiController
    {
		public int Create(Order order)
        {
            throw new NotImplementedException();
        }
    }
	
	public class Order
    {
        public int Id { get; set; }
        public IEnumerable<LineItem> LineItems { get; set; }
    }

	public class LineItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
