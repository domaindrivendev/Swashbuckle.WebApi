using System.Collections.ObjectModel;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public interface IApiDescriptionFilter
    {
        Collection<ApiDescription> Apply(IApiExplorer apiExplorer);
    }
}