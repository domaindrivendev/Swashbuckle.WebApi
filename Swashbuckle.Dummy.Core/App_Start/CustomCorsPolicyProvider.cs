using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace Swashbuckle.Dummy.App_Start
{
    public class CustomCorsPolicyProvider : ICorsPolicyProvider
    {
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corsPolicy = new CorsPolicy();

            if (request.RequestUri.PathAndQuery.StartsWith("/swagger/docs"))
            {
                corsPolicy.AllowAnyHeader = true;
                corsPolicy.AllowAnyMethod = true;
                corsPolicy.AllowAnyOrigin = true;
            }

            var tsc = new TaskCompletionSource<CorsPolicy>(); 
            tsc.SetResult(corsPolicy);
            return tsc.Task;
        }
    }
}
