using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Hwavmvid.Jsapigeolocation
{
    public class Jsapigeolocationmap
    {
        public string Id { get; set; }
        public Jsapigeolocationitem Item { get; set; }
        public IJSObjectReference Jsmapreference { get; set; }
        public string Permissionstate { get; set; }
    }
}
