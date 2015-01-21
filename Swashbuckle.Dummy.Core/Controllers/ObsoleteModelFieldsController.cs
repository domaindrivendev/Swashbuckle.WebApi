using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ObsoleteModelFieldsController : ApiController
    {
        public int Create(CreateEntityForm form)
        {
            throw new NotImplementedException();
        }
    }
    
    public class CreateEntityForm
    {
        [Obsolete("We switched to generated ids -- clients should only provide Name")]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}