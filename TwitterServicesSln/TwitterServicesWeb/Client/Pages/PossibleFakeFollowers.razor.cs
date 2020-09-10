using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TwitterServicesWeb.Shared;

namespace TwitterServicesWeb.Client.Pages
{
    public partial class PossibleFakeFollowers
    {
        private List<PossibleFakeUserModel> FollowersList { get; set; }
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string userInput;
        private string messageInput;
        [Inject]
        private NavigationManager NavManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //Note: This is currently failing in .NET 5, with n X509 known issue: "System.Security.Cryptography is not supported on this platform"
            hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/possiblefakefollowershub"))
                .Build();

            hubConnection.On<PossibleFakeUserModel>("OnNewPossibleFakeUserFound", (userFound) =>
            {
                if (this.FollowersList == null)
                    this.FollowersList = new List<PossibleFakeUserModel>();
                this.FollowersList.Add(userFound);
                StateHasChanged();
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
        }

    }
}
