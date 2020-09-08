using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PTI.TwitterServices.Models;
using TwitterServicesWeb.Server.Helpers;
using TwitterServicesWeb.Shared;

namespace TwitterServicesWeb.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TwitterController : ControllerBase
    {
        private IMemoryCache MemoryCache { get; }
        public TwitterController(IMemoryCache memoryCache)
        {
            this.MemoryCache = memoryCache;
        }

        [HttpGet("[action]")]
        public IActionResult GetPossibleFakeFollowers()
        {
            this.MemoryCache.TryGetValue<List<PossibleFakeUserModel>>(Constants.PossibleFakeUsers, 
                out List<PossibleFakeUserModel> users);
            return Ok(users);
        }
    }
}
