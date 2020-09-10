using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitterServicesWeb.Shared;

namespace PTI.BackgroundServices.SignalR
{
    public class PossibleFakeFollowersHub: Hub<PossibleFakeFollowersHubClient>
    {
        public async Task NotifyAll(PossibleFakeUserModel userModel)
        {
            this.Clients.All.OnNewPossibleFakeUserFound(userModel);
        }
    }

    public class PossibleFakeFollowersHubClient
    {
        public async Task OnNewPossibleFakeUserFound(PossibleFakeUserModel userModel)
        {

        }
    }
}
