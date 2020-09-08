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
    public class BaseTwitterServiceTests: TwitterServicesTestsBase
    {

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

        [TestMethod]
        public async Task Test_GetFollowersByUsername()
        {
            string username = this.BaseTwitterServiceConfiguration.ScreenName;
            var result = await this.BaseTwitterService.GetUserFollowersByUsernameAsync(username);
            Assert.IsNotNull(result);
        }
    }
}
