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

namespace PTI.BackgroundServices
{
    public class TwitterBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        private FakeFollowersTwitterService FakeFollowersTwitterService { get; }
        private IMemoryCache MemoryCache { get; }
        private ILogger Logger { get; }

        public TwitterBackgroundService(PTI.TwitterServices.Services.FakeFollowersTwitterService fakeFollowersTwitterService,
            BaseTwitterServiceConfiguration baseTwitterServiceConfiguration,
            IMemoryCache memoryCache,
            ILogger logger)
        {
            this.BaseTwitterServiceConfiguration = baseTwitterServiceConfiguration;
            this.FakeFollowersTwitterService = fakeFollowersTwitterService;
            this.MemoryCache = memoryCache;
            this.Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Yield();
                List<PossibleFakeUserModel> lstAllPossibleFakeUsers = new List<PossibleFakeUserModel>();
                Action<PossibleFakeUser> onNewFakeUserFound = (user) =>
                {
                    lstAllPossibleFakeUsers.Add(new PossibleFakeUserModel()
                    {
                        Username = user.User.ScreenNameResponse,
                        Reasons = user.PossibleFakeReasons.Select(p => p.ToString()).ToList(),
                        ProfileUrl = $"https://www.twitter.com/{user.User.ScreenNameResponse}"
                    });
                    this.Logger?.LogInformation($"New users added to main list: Total users: {lstAllPossibleFakeUsers.Count()}");
                    this.MemoryCache.Set<List<PossibleFakeUserModel>>(Constants.PossibleFakeUsers, lstAllPossibleFakeUsers);
                };
                await this.FakeFollowersTwitterService
                    .GetAllPossibleFakeFollowersForUsernameAsync(this.BaseTwitterServiceConfiguration.ScreenName,
                    onNewFakeUserFound)
                    .ConfigureAwait(continueOnCapturedContext: true);
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
        }
    }
}
