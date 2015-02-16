using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class IndexerTypesController : ApiController
    {
        public int Create(Lookup lookup)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Lookup
    {
        public int TotalEntries { get; set; }

        public string this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public string this[int index]
        {
            get { throw new NotImplementedException(); }
        }
    }
}