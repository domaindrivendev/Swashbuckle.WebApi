using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class FileDownloadController : ApiController
    {
        public HttpResponseMessage GetFile()
        {
            var response = new HttpResponseMessage();
            var stream = new FileStream(@"c:\test.txt", FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return response;
        }
    }
}
