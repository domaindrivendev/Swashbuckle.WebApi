using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class CustomActionNamesController : ApiController
    {       
        /// <summary>
        /// Test ActionName attribute
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("TestActionName")]
        public void DifferentMethodName(int id)
        {
            throw new NotImplementedException();
        }
    }
}
