using System.Web.Http;

namespace Swagger_Test.Controllers
{
    public abstract class Blob<T> : ApiController
    {
        /// <summary> Get a Bad Blob </summary>
        public string GetBad(int? x) { return "Bad"; }

        /// <summary> Get an Ok Blob </summary>
        public string PostOk(int x) { return "Ok"; }
    }

    public class Foo { }

    public class BlobController : Blob<Foo> { }
}