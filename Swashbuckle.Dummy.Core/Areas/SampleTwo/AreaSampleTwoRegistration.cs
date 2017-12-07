using System.Web.Mvc;

namespace Swashbuckle.Dummy.Areas.SampleTwo
{
    public class AreaSampleTwoRegistration : AreaRegistration
    {
        public override string AreaName => "SampleTwo";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SampleTwoApi_default",
                "sampletwo/api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
