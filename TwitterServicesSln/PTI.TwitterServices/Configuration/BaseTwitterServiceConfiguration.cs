using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.TwitterServices.Configuration
{
    /// <summary>
    /// Configuration for the BaseTwitterService
    /// </summary>
    public sealed class BaseTwitterServiceConfiguration
    {
        /// <summary>
        /// Twitter app Access Token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Twitter App Access Token Secret
        /// </summary>
        public string AccessTokenSecret { get; set; }
        /// <summary>
        /// Twitter App Consumer Key
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// Twitter App Consumer Secret
        /// </summary>
        public string ConsumerSecret { get; set; }
        /// <summary>
        /// Screen name of the user to log in with/identify as
        /// </summary>
        public string ScreenName { get; set; }
        /// <summary>
        /// Id of the user to log in with/identify as
        /// </summary>
        public ulong UserId { get; set; }
        /// <summary>
        /// Maximum number of times an operation is going to be retried
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;
        /// <summary>
        /// Indicated if failed operations should be retried
        /// </summary>
        public bool RetryOperationOnFailure { get; set; } = true;
    }
}
