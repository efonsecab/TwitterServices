using LinqToTwitter;
using Microsoft.Extensions.Logging;
using PTI.TwitterServices.Configuration;
using PTI.TwitterServices.Enums;
using PTI.TwitterServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.TwitterServices.Services
{
    /// <summary>
    /// Service to help find possible Twitter fake followers
    /// </summary>
    public class FakeFollowersTwitterService
    {
        private BaseTwitterService BaseTwitterService { get; }
        private ILogger Logger { get; }
        /// <summary>
        /// Default service constructor
        /// </summary>
        /// <param name="baseTwitterService"></param>
        /// <param name="logger"></param>
        public FakeFollowersTwitterService(BaseTwitterService baseTwitterService,
            ILogger logger)
        {
            this.BaseTwitterService = baseTwitterService;
            this.Logger = logger;
        }

        /// <summary>
        /// Retrieve a list of all possible fake followers for a given username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="onNewPossibleFakeFollowedDetectedAction">Action to be executed when a new possible fake has been found.
        /// Used so that consumer does not have to wait for the whole process to finish</param>
        /// <returns></returns>
        public async Task<List<User>> GetAllPossibleFakeFollowersForUsernameAsync(string username,
            Action<User> onNewPossibleFakeFollowedDetectedAction = null)
        {
            List<PossibleFakeUser> lstPossibleFakeFollowers = new List<PossibleFakeUser>();
            var userFollowers = await this.BaseTwitterService.GetUserFollowersByUsernameAsync(username);
            long? currentFollowersCursor = null;
            Action<User, List<PossibleFakeReason>> onPossibleFakeFollowerAction = (user, reasons) =>
            {
                lstPossibleFakeFollowers.Add(new PossibleFakeUser() 
                {
                    User = user,
                    PossibleFakeReasons = reasons
                });
                if (onNewPossibleFakeFollowedDetectedAction != null)
                    onNewPossibleFakeFollowedDetectedAction(user);
            };
            if (userFollowers != null && userFollowers.Users != null && userFollowers.Users.Count > 0)
            {
                currentFollowersCursor = userFollowers.CursorMovement?.Next;
                foreach (var singleFollower in userFollowers.Users)
                {
                    await EvaluateIfPossibleFakeFollower(singleFollower, onPossibleFakeFollowerAction);
                }
                if (currentFollowersCursor != null)
                {
                    do
                    {
                        userFollowers = await this.BaseTwitterService.GetUserFollowersByUsernameAsync(username, cursor: currentFollowersCursor.Value);
                        currentFollowersCursor = userFollowers?.CursorMovement?.Next;
                        foreach (var singleFollower in userFollowers.Users)
                        {
                            await EvaluateIfPossibleFakeFollower(singleFollower, onPossibleFakeFollowerAction);
                        }
                    }
                    while (userFollowers != null && userFollowers.Users != null && userFollowers.Users.Count > 0);
                }
            }
            return lstPossibleFakeFollowers;
        }


        private async Task EvaluateIfPossibleFakeFollower(LinqToTwitter.User singleFollower,
            Action<User, List<PossibleFakeReason>> onPossibleFakeFollowerAction)
        {
            var lstPossibleFakeReasons = await GetPossibleFakeReasons(singleFollower);
            if (lstPossibleFakeReasons != null && lstPossibleFakeReasons.Count() > 0)
            {
                if (onPossibleFakeFollowerAction != null)
                    onPossibleFakeFollowerAction(singleFollower, lstPossibleFakeReasons);
            }
        }

        private async Task<List<PossibleFakeReason>> GetPossibleFakeReasons(User singleFollower)
        {
            List<PossibleFakeReason> lstPossibleFakeReasons = new List<PossibleFakeReason>();
            var lastOriginalTweet =
                (await this.BaseTwitterService.GetTweetsByUsernameAsync(singleFollower.ScreenNameResponse, 10, isRetweeted: false))
                .FirstOrDefault();
            bool hasEmptyDescription = String.IsNullOrWhiteSpace(singleFollower.Description);
            if (hasEmptyDescription)
                lstPossibleFakeReasons.Add(PossibleFakeReason.EmptyBioDescription);
            if (lastOriginalTweet != null)
            {
                var timeSinceOriginalRetweet = DateTimeOffset.UtcNow.Subtract(lastOriginalTweet.CreatedAt);
                var timeSinceUserCreation = DateTimeOffset.UtcNow.Subtract(singleFollower.CreatedAt);
                if (timeSinceOriginalRetweet.TotalDays > 15 &&
                    //about 8 months
                    timeSinceUserCreation.TotalDays > 240
                    )
                {
                    lstPossibleFakeReasons.Add(PossibleFakeReason.LongTimeWithoutOriginalTweets);
                }
            }
            bool hasEmptyProfileImage = String.IsNullOrWhiteSpace(singleFollower.ProfileImageUrl) ||
                String.IsNullOrWhiteSpace(singleFollower.ProfileImageUrlHttps);
            if (hasEmptyProfileImage)
                lstPossibleFakeReasons.Add(PossibleFakeReason.EmptyProfileImage);
            if (singleFollower.FollowersCount > singleFollower.FriendsCount)
            {

            }
            else
            {
                double expectedFollowersOffset = singleFollower.FriendsCount * 0.4;
                if (singleFollower.FollowersCount < expectedFollowersOffset)
                {
                    lstPossibleFakeReasons.Add(PossibleFakeReason.InvalidFollowersOffset);
                }
            }
            return lstPossibleFakeReasons;
        }
    }
}
