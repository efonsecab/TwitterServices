using Microsoft.Extensions.Configuration;
using PTI.TwitterServices.Configuration;
using PTI.TwitterServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterServices.AutomatedTests.Services
{
    public class TwitterServicesTestsBase
    {
        protected BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        protected BaseTwitterService BaseTwitterService { get; }
        protected FakeFollowersTwitterService FakeFollowersTwitterService { get; }

        public TwitterServicesTestsBase()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddUserSecrets("c6c434c5-169d-4022-9a92-3005cc5cc363");
            var configuration = configurationBuilder.Build();
            BaseTwitterServiceConfiguration baseTwitterServiceConfiguration =
                configuration.GetSection("BaseTwitterServiceConfiguration").Get<BaseTwitterServiceConfiguration>();
            this.BaseTwitterServiceConfiguration = baseTwitterServiceConfiguration;
            PTI.TwitterServices.Services.BaseTwitterService baseTwitterService =
                new PTI.TwitterServices.Services.BaseTwitterService(this.BaseTwitterServiceConfiguration, null);
            this.BaseTwitterService = baseTwitterService;
            PTI.TwitterServices.Services.FakeFollowersTwitterService fakeFollowersTwitterService =
                new FakeFollowersTwitterService(this.BaseTwitterService,null);
            this.FakeFollowersTwitterService = fakeFollowersTwitterService;
        }
    }
}
