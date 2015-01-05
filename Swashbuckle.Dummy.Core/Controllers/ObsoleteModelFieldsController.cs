using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ObsoleteModelFieldsController : ApiController
    {
        static int _generatedId;

        public int Create(CreateEntityForm form)
        {
            return _generatedId++;
        }
    }
    
    public class CreateEntityForm
    {
        [Obsolete("We switched to generated ids -- clients should only provide Name")]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}