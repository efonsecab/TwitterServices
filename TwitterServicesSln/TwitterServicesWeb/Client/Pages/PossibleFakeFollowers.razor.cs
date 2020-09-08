using Microsoft.AspNetCore.Components;
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
        [Inject]
        private HttpClient HttpClient { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.FollowersList =
                    await this.HttpClient.GetFromJsonAsync<List<PossibleFakeUserModel>>("Twitter/GetPossibleFakeFollowers");
            }
            catch (Exception)
            {

            }
        }
    }
}
