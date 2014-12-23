using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class XmlAnnotatedController : ApiController
    {
        /// <summary>
        /// Registers a new Account
        /// </summary>
        /// <remarks>Create an account to access restricted resources</remarks>
        /// <param name="account">Details for the account to be created</param>
        public int Create(Account account)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a SubAccount.
        /// </summary>
        [HttpPut]
        public int UpdateSubAccount(SubAccount account)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Search all registered accounts by keywords 
        /// </summary>
        /// <remarks>Restricted to admin users only</remarks>
        /// <param name="keywords">List of search keywords</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Account> Search(IEnumerable<string> keywords)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Account details
    /// </summary>
    public class Account
    {
        /// <summary>
        /// The ID for Accounts is 5 digits long.
        /// </summary>
        public virtual int AccountID { get; set; }
        
        /// <summary>
        /// Uniquely identifies the account
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// For authentication
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// A Sub-Type of Account
    /// </summary>
    public class SubAccount : Account
    {

        /// <summary>
        /// The Account ID for SubAccounts should be 7 digits.
        /// </summary>
        public override int AccountID { get; set; }

    }
}