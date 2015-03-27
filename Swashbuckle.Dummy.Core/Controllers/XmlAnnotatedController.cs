using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class XmlAnnotatedController : ApiController
    {
        /// <summary>
        /// Registers a new Account based on <paramref name="account"/>.
        /// </summary>
        /// <remarks>Create an <see cref="Account"/> to access restricted resources</remarks>
        /// <param name="account">Details for the account to be created</param>
        /// <response code="201"><paramref name="account"/> created</response>
        /// <response code="400">Username already in use</response>
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

        /// <summary>
        /// Prevents the account from being used
        /// </summary>
        [HttpPut]
        [ActionName("put-on-hold")]
        public void PutOnHold(int id)
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

        public AccountPreferences Preferences { get; set; }

        public class AccountPreferences
        {
            /// <summary>
            /// Provide a display name to use instead of Username when signed in
            /// </summary>
            public string DisplayName { get; set; }
        }
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