using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Web.Http;
using System.Collections.Generic;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class AddOAuth2Scopes : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            var scopeIds = apiDescription.ActionDescriptor.GetFilterPipeline()
                .Select(filterInfo => filterInfo.Instance)
                .OfType<ScopeAuthorizeAttribute>()
                .SelectMany(attr => attr.Scopes)
                .Distinct();

            if (scopeIds.Any())
            {
                operation.Authorizations = new Dictionary<string, IList<Scope>>();
                operation.Authorizations["oauth2"] = scopeIds
                    .Select(id => new Scope { ScopeId = id })
                    .ToList();
            }
        }
    }

    public class ScopeAuthorizeAttribute : AuthorizeAttribute
    {
        public ScopeAuthorizeAttribute(params string[] scopes)
        {
            Scopes = scopes;
        }

        public string[] Scopes { get; private set; }
    }
}