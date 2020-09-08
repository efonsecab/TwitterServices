using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterServicesWeb.Shared
{
    public class PossibleFakeUserModel
    {
        public string Username { get; set; }
        public List<string> Reasons { get; set; }
        public string ProfileUrl { get; set; }
    }
}
