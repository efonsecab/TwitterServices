using LinqToTwitter;
using Microsoft.Extensions.Logging;
using PTI.TwitterServices.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.TwitterServices.Services
{
    /// <summary>
    /// Service containing the basic Twitter functionality
    /// </summary>
    public class BaseTwitterService
    {
        private ILogger Logger { get; }
        private BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        /// <summary>
        /// Twitter connection context
        /// </summary>
        protected TwitterContext TwitterContext { get; }

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
                            ConsumerKey = this.BaseTwitterServiceConfiguration.ConsumerKey,
                            ConsumerSecret = this.BaseTwitterServiceConfiguration.ConsumerSecret,
                            ScreenName = this.BaseTwitterServiceConfiguration.ScreenName
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
        public async Task<List<Status>> GetTweetsByUserIdAsync(ulong userId, int? maxTweets = 10, ulong? sinceTweetId = 1)
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
        /// <param name="isRetweeted"></param>
        /// <returns></returns>
        public async Task<List<Status>> GetTweetsByUsernameAsync(string username, int maxTweets = 10, ulong sinceTweetId = 1,
            bool isRetweeted = false)
        {
            List<Status> lstTweets = null;
            bool mustStop = false;
            int currentRetries = 0;
            do
            {
                try
                {
                    this.Logger.LogInformation($"Scanning message for: {username}. Current Retries: {currentRetries}");
                    lstTweets = await this.TwitterContext.Status.Where(p =>
                    p.Type == StatusType.User &&
                    p.ScreenName == username &&
                    p.Count == maxTweets &&
                    p.SinceID == sinceTweetId &&
                    p.Retweeted == isRetweeted
                    ).ToListAsync();
                    mustStop = true;

                }
                catch (Exception ex)
                {
                    this.Logger?.LogError(ex, ex.Message);
                    this.Logger?.LogError(ex, ex.Message);
                    if (this.BaseTwitterServiceConfiguration.RetryOperationOnFailure && currentRetries < this.BaseTwitterServiceConfiguration.MaxRetryCount)
                    {
                        currentRetries++;
                        await EvaluateIfRateLimitExceededAsync();
                    }
                    else
                        mustStop = true;
                }
            }
            while (!mustStop);
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

        /// <summary>
        /// Retrieves the followers for a specified user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="maxFollowers"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public async Task<Friendship> GetUserFollowersByUsernameAsync(string username, int maxFollowers = 10,
            long? cursor = null)
        {
            Friendship followers = null;
            bool mustStop = false;
            int currentRetries = 0;
            do
            {
                try
                {
                    this.Logger?.LogInformation($"Scanning followers for: {username}. Cursor: {cursor}. Current Retries: {currentRetries}");
                    await this.EvaluateIfRateLimitExceededAsync();
                    var followersQuery = this.TwitterContext.Friendship.Where(p =>
                    p.Type == FriendshipType.FollowersList &&
                    p.ScreenName == username &&
                    p.Count == maxFollowers
                    );
                    if (cursor != null)
                        followersQuery = followersQuery.Where(p => p.Cursor == cursor);
                    followers = await followersQuery.SingleOrDefaultAsync();
                    mustStop = true;
                }
                catch (Exception ex)
                {
                    this.Logger?.LogError(ex, ex.Message);
                    if (this.BaseTwitterServiceConfiguration.RetryOperationOnFailure && currentRetries < this.BaseTwitterServiceConfiguration.MaxRetryCount)
                    {
                        currentRetries++;
                        await EvaluateIfRateLimitExceededAsync();
                    }
                    else
                        mustStop = true;
                }
            } while (!mustStop);
            return followers;
        }

        /// <summary>
        /// Evaluates if Rate Limit has been excedded and waits the specified time by twitter api
        /// </summary>
        protected async Task EvaluateIfRateLimitExceededAsync()
        {
            if (this.TwitterContext.RateLimitRemaining == 0)
            {
                var d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                d = d.AddSeconds(this.TwitterContext.RateLimitReset);
                var timeToWait = d.Subtract(DateTime.UtcNow);
                int iMillisecondsToWait = (int)Math.Ceiling(timeToWait.TotalMilliseconds);
                if (iMillisecondsToWait < -1)
                {
                    iMillisecondsToWait = 30 * 1000; //30 seconds
                }
                var totalMinutes = TimeSpan.FromMilliseconds(iMillisecondsToWait).TotalMinutes;
                var resumeTime = DateTime.Now.AddMinutes(totalMinutes);
                this.Logger?.LogInformation($"Reached Twitters APIs Limits. Waiting for: {totalMinutes} minutes. " +
                    $"Resuming at local time: {resumeTime}");
                await Task.Delay(iMillisecondsToWait);
            }
        }
    }
}
