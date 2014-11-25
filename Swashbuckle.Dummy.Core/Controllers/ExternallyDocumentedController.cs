using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public interface IExternallyDocumentedController
    {
        /// <summary>
        /// This summary is defined on interface level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetSimpleValue(int id);

        /// <summary>
        /// This summary is defined on interface level and will be overriden on implementation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        void SetSimpleValue(int id, int value);
    }

    public class ExternallyDocumentedController : ApiController, IExternallyDocumentedController
    {
        public int GetSimpleValue(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This summary is defined on instance and overrides interface
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetSimpleValue(int id, int value)
        {
            throw new NotImplementedException();
        }
    }
}
