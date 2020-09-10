using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PTI.BackgroundServices.Helpers;
using PTI.TwitterServices.Configuration;
using PTI.TwitterServices.Models;
using PTI.TwitterServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitterServicesWeb.Shared;
using PTI.BackgroundServices.SignalR;

namespace PTI.BackgroundServices
{
    public class TwitterBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        private FakeFollowersTwitterService FakeFollowersTwitterService { get; }
        private IMemoryCache MemoryCache { get; }
        private ILogger Logger { get; }
        public IHubContext<PossibleFakeFollowersHub> HubContext { get; }

        public TwitterBackgroundService(PTI.TwitterServices.Services.FakeFollowersTwitterService fakeFollowersTwitterService,
            BaseTwitterServiceConfiguration baseTwitterServiceConfiguration,
            IMemoryCache memoryCache,
            ILogger logger, IHubContext<PossibleFakeFollowersHub> hubContext)
        {
            this.BaseTwitterServiceConfiguration = baseTwitterServiceConfiguration;
            this.FakeFollowersTwitterService = fakeFollowersTwitterService;
            this.MemoryCache = memoryCache;
            this.Logger = logger;
            this.HubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //if we rely on hub only sending the latest modified value, we must wait, or we can set the hub to only send a notification
                //and have the client invoke the api to retrieve the update list of users, which are stored in memory cache.
                await Task.Delay(TimeSpan.FromMinutes(2));
                List<PossibleFakeUserModel> lstAllPossibleFakeUsers = new List<PossibleFakeUserModel>();
                Action<PossibleFakeUser> onNewFakeUserFound = async (user) =>
                {
                    var model = new PossibleFakeUserModel()
                    {
                        Username = user.User.ScreenNameResponse,
                        Reasons = user.PossibleFakeReasons.Select(p => p.ToString()).ToList(),
                        ProfileUrl = $"https://www.twitter.com/{user.User.ScreenNameResponse}"
                    };
                    lstAllPossibleFakeUsers.Add(model);
                    await this.HubContext.Clients.All.SendAsync("OnNewPossibleFakeUserFound", model);
                    this.Logger?.LogInformation($"New users added to main list: Total users: {lstAllPossibleFakeUsers.Count()}");
                    this.MemoryCache.Set<List<PossibleFakeUserModel>>(Constants.PossibleFakeUsers, lstAllPossibleFakeUsers);
                };
                onNewFakeUserFound(new PossibleFakeUser() 
                {
                    PossibleFakeReasons = new List<TwitterServices.Enums.PossibleFakeReason>()
                    { TwitterServices.Enums.PossibleFakeReason.EmptyBioDescription, TwitterServices.Enums.PossibleFakeReason.InvalidFollowersOffset},
                    User = new LinqToTwitter.User()
                    {
                        ScreenNameResponse="User"
                    }
                });
                //await this.FakeFollowersTwitterService
                //    .GetAllPossibleFakeFollowersForUsernameAsync(this.BaseTwitterServiceConfiguration.ScreenName,
                //    onNewFakeUserFound)
                //    .ConfigureAwait(continueOnCapturedContext: true);
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
        }
    }
}
