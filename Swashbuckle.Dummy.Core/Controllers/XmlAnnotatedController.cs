using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class XmlAnnotatedController : ApiController
    {
        /// <summary>
        /// Registers a new Account
        /// </summary>
        /// <remarks>With a registered account, you can access restricted resources</remarks>
        public int Create(Account account)
        {
            throw new NotImplementedException();
        }
    }

    public class Account
    {}
}