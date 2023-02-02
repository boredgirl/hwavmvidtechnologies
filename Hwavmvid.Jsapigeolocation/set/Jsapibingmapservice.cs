using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hwavmvid.Jsapigeolocation
{

    public class Jsapibingmapservice : IDisposable
    {
        
        public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference Module { get; set; } = null;
        public DotNetObjectReference<Jsapibingmapservice> DotNetObjectRef { get; set; }

        public event Action OnUpdateUI;

        public List<Jsapibingmapmap> Mapitems { get; set; } = new List<Jsapibingmapmap>();

        public Jsapibingmapservice(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.DotNetObjectRef = DotNetObjectReference.Create(this);
        }
        public async Task Initbingmapservice()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/jsapibingmapjsinterop.js");
            }
        }
        public async Task Initbingmapmap(string componentid, string elementid)
        {
            var contextmap = this.Getmap(componentid);
            if (contextmap == null)
            {
                contextmap = new Jsapibingmapmap() { Id = componentid, Item = null, Jsmapreference = null };
                contextmap.Jsmapreference = await this.Module.InvokeAsync<IJSObjectReference>("initbingmapmap", this.DotNetObjectRef, componentid, elementid);
                this.Mapitems.Add(contextmap);
            }                
        }

        public Jsapibingmapmap Getmap(string id)
        {
            return this.Mapitems.FirstOrDefault(item => item.Id == id);
        }
        public void Removemap(string componentid)
        {
            var map = this.Getmap(componentid);
            if (map != null)
                this.Mapitems.Remove(map);
        }

        public async Task Renderbingmapposition(string componentid, double? latitude, double? longitude)
        {
            var map = this.Getmap(componentid);
            if (map != null)
                await map.Jsmapreference.InvokeVoidAsync("renderbingmapposition", latitude, longitude);
        }

        public void UpdateUI()
        {
            this.OnUpdateUI?.Invoke();
        }

        public void Dispose()
        {
            if (this.Module != null)
            {
                this.Module.DisposeAsync();
            }
        }

    }
}