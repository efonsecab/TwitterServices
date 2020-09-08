using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PTI.TwitterServices.Configuration;
using PTI.TwitterServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterServices.AutomatedTests.Services
{
    [TestClass]
    public class BaseTwitterServiceTests
    {
        private BaseTwitterServiceConfiguration BaseTwitterServiceConfiguration { get; }
        private BaseTwitterService BaseTwitterService { get; }

        public BaseTwitterServiceTests()
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
        }

        [TestMethod]
        public async Task Test_GetTweetsByUsername()
        {
            string username = "efonsecabcr";
            var result = await this.BaseTwitterService.GetTweetsByUsernameAsync(username);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_GetTweetsByUserId()
        {
            ulong userId = this.BaseTwitterServiceConfiguration.UserId;
            var result = await this.BaseTwitterService.GetTweetsByUserIdAsync(userId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Test_GetUserInfoByUsername()
        {
            string username = this.BaseTwitterServiceConfiguration.ScreenName;
            var result = await this.BaseTwitterService.GetUserInfoByUsernameAsync(username);
            Assert.IsNotNull(result);
        }
    }
}
