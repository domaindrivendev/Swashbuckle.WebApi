using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;

namespace Swashbuckle.Dummy.Controllers
{
    [RoutePrefix("xmlannotated")]
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
        /// Filters account based on the given parameters.
        /// </summary>
        /// <param name="q">The search query on which to filter accounts</param>
        /// <param name="page">A complex type describing the paging to be used for the request</param>
        /// <returns></returns>
        [HttpGet]
        [Route("filter")]
        public IEnumerable<Account> Filter(string q, [FromUri]Page page)
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

        /// <summary>
        /// Adds a reward to an existing account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reward"></param>
        [HttpPut]
        [Route("{id}/add-reward")]
        public void AddReward(int id, Reward<string> reward)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates metadata associated with an account 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        [HttpPut]
        [Route("{id}/metadata")]
        public void UpdateMetadata(int id, KeyValuePair<string, string>[] metadata)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Markdown Indention Tests
        /// </summary>
        /// <remarks>First Line after remarks tag
        /// Second no additional space
        ///      Third with extra leading spaces (that should be ignored)
        ///      
        /// .    Fourth with intentional leading spaces</remarks>
        [HttpGet]
        [Route("markdown")]
        public void MarkDownTest()
        {

        }
    }

    public class Page
    {
        /// <summary>
        /// The maximum number of accounts to return
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Offset into the result
        /// </summary>
        public int Offset { get; set; }
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

            /// <summary>
            /// Flag to indicate if marketing emails may be sent
            /// </summary>
            [JsonProperty("allow-marketing-emails")]
            public string AllowMarketingEmails { get; set; }            
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

    /// <summary>
    /// A redeemable reward
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Reward<T>
    {
        /// <summary>
        /// The monetary value of the reward 
        /// </summary>
        public decimal value;

        /// <summary>
        /// The reward type
        /// </summary>
        public T RewardType { get; set; }
    }
}