using System.Web.Mvc;

namespace Swashbuckle.Dummy.Areas.SampleOne
{
    public class AreaSampleOneRegistration : AreaRegistration
    {
        public override string AreaName => "SampleOne";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SampleOneApi_default",
                "sampleone/api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
