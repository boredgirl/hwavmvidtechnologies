using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Hwavmvid.Jsapigeolocation
{
    public class Jsapibingmapmap
    {
        public string Id { get; set; }
        public Jsapibingmapitem Item { get; set; }
        public IJSObjectReference Jsmapreference { get; set; }
        public string Permissionstate { get; set; }
    }
}
