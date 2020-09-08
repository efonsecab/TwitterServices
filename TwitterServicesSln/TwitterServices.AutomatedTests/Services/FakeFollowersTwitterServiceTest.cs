using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterServices.AutomatedTests.Services
{
    [TestClass]
    public class FakeFollowersTwitterServiceTest: TwitterServicesTestsBase
    {
        [TestMethod]
        public async Task GetAllPossibleFakeFollowersForUsername()
        {
            string username = this.BaseTwitterServiceConfiguration.ScreenName;
            var allPossibleFakeFollowers = 
                await this.FakeFollowersTwitterService.GetAllPossibleFakeFollowersForUsernameAsync(username);
            Assert.IsTrue(allPossibleFakeFollowers.Count() > 0);
        }
    }
}
