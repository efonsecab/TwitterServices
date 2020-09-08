using LinqToTwitter;
using PTI.TwitterServices.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PTI.TwitterServices.Services.FakeFollowersTwitterService;

namespace PTI.TwitterServices.Models
{
    /// <summary>
    /// Combines the User information with the found possible fake reasons
    /// </summary>
    public class PossibleFakeUser
    {
        /// <summary>
        /// User information
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// Reasons why the user was identified as a possible fake
        /// </summary>
        public List<PossibleFakeReason> PossibleFakeReasons { get; set; }
    }
}
