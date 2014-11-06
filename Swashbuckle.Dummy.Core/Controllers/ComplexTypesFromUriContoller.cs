using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ComplexTypesFromUriController : ApiController
    {
        public string Get([FromUri] Model1 model1, [FromUri] Model2 model2)
        {
            return string.Format("param1={0}&param2={1}&param3={2}&param4={3}", model1.Param1, model1.Param2, model2.Param3, model2.Param4);
        }
    }

    public class Model1
    {
        public string Param1 { get; set; }

        public int Param2 { get; set; }
    }

    public class Model2
    {
        [Required]
        public string Param3 { get; set; }

        public int Param4 { get; set; }
    }
}