using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class SelfReferencingTypesController : ApiController
    {
		public int Create(Component component)
        {
            throw new NotImplementedException();
        }
    }
	
	public class Component
    {
        public string Name { get; set; }
        public IEnumerable<Component> SubComponents { get; set; }
    }
}
