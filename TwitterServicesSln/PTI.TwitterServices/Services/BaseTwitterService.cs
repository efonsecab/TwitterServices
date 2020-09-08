using LinqToTwitter;
using Microsoft.Extensions.Logging;
using PTI.TwitterServices.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.TwitterServices.Services
{
    /// <summary>
    /// Service containing the basic Twitter functionality
    /// </summary>
    public sealed class BaseTwitterService
    {
        private ILogger Logger { get; }
        private BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        private TwitterContext TwitterContext { get; }

        /// <summary>
        /// Default service constructor
        /// </summary>
        /// <param name="baseTwitterServiceConfiguration">Base Twitter Configuration</param>
        /// <param name="logger">Logger</param>
        public BaseTwitterService(BaseTwitterServiceConfiguration baseTwitterServiceConfiguration, 
            ILogger logger)
        {
            this.Logger = logger;
            this.BaseTwitterServiceConfiguration = baseTwitterServiceConfiguration;
            LinqToTwitter.TwitterContext twitterContext =
                new LinqToTwitter.TwitterContext(
                    new LinqToTwitter.SingleUserAuthorizer()
                    {
                        CredentialStore = new LinqToTwitter.SingleUserInMemoryCredentialStore()
                        {
                            AccessToken = this.BaseTwitterServiceConfiguration.AccessToken,
                            AccessTokenSecret = this.BaseTwitterServiceConfiguration.AccessTokenSecret,
                            ConsumerKey= this.BaseTwitterServiceConfiguration.ConsumerKey,
                            ConsumerSecret=this.BaseTwitterServiceConfiguration.ConsumerSecret,
                            ScreenName=this.BaseTwitterServiceConfiguration.ScreenName
                        }
                    });
            this.TwitterContext = twitterContext;
        }

        /// <summary>
        /// Retrieves a list of the latest tweets for the specified user id
        /// </summary>
        /// <param name="userId">Id of the user to retrieve tweets from</param>
        /// <param name="maxTweets">Maximum number of items to retrieve. Default is 10</param>
        /// <param name="sinceTweetId">Tweets it to start from</param>
        /// <returns></returns>
        public async Task<List<Status>> GetTweetsByUserIdAsync(ulong userId,int? maxTweets=10, ulong? sinceTweetId=1)
        {
            List<Status> lstTweets = null;
            try
            {
                lstTweets = await this.TwitterContext.Status.Where(p =>
                p.Type == StatusType.User &&
                p.UserID == userId &&
                p.Count == maxTweets &&
                p.SinceID == sinceTweetId
                ).ToListAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
            return lstTweets;
        }

        /// <summary>
        /// Retrieves a list of the latest tweets for the specified user id
        /// </summary>
        /// <param name="username">username to retrieve tweets from</param>
        /// <param name="maxTweets">Maximum number of items to retrieve. Default is 10</param>
        /// <param name="sinceTweetId">Tweets it to start from</param>
        /// <returns></returns>
        public async Task<List<Status>> GetTweetsByUsernameAsync(string username, int? maxTweets = 10, ulong? sinceTweetId = 1)
        {
            List<Status> lstTweets = null;
            try
            {
                lstTweets = await this.TwitterContext.Status.Where(p =>
                p.Type == StatusType.User &&
                p.ScreenName == username &&
                p.Count == maxTweets &&
                p.SinceID == sinceTweetId
                ).ToListAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
            return lstTweets;
        }

        /// <summary>
        /// Retrieves the information for a specified username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User> GetUserInfoByUsernameAsync(string username)
        {
            User userInfo = null;
            try
            {
                userInfo = await this.TwitterContext.User.Where(p =>
                p.Type == UserType.Show &&
                p.ScreenName == username
                ).SingleOrDefaultAsync();
                return userInfo;
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
            return userInfo;
        }
    }
}
