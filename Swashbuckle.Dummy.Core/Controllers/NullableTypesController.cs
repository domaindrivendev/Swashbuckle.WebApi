using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class NullableTypesController : ApiController
    {
        public int Create(Contact contact)
        {
            throw new NotImplementedException();
        }
    }

    public class Contact
    {
        public string Name { get; set; }

        public int? Phone { get; set; }
    }
}