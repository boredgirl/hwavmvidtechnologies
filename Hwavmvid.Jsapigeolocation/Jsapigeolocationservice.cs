using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hwavmvid.Jsapigeolocation
{

    public class Jsapigeolocationservice : IDisposable
    {
        
        public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference Module { get; set; }

        public DotNetObjectReference<Jsapigeolocationservice> DotNetObjectRef;

        public event Action<Jsapigeolocationpermissionsevent> OnGeolocationpermisssionsChanged;
        public event Action OnUpdateUI;

        public List<Jsapigeolocationmap> Mapitems { get; set; } = new List<Jsapigeolocationmap>();

        public Jsapigeolocationservice(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.DotNetObjectRef = DotNetObjectReference.Create(this);
            this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/geolocationscript.js");
        }
        public async Task Initgeolocationservice()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/jsapigeolocationjsinterop.js");
            }
        }
        public async Task InitGeolocationMap(string componentid, string elementid)
        {
            var contextmap = this.Getmap(componentid);
            if (contextmap == null)
            {
                contextmap = new Jsapigeolocationmap() { Id = componentid, Item = null };
                contextmap.Jsmapreference = await this.Module.InvokeAsync<IJSObjectReference>("initgeolocationmap", this.DotNetObjectRef, componentid, elementid);
                this.Mapitems.Add(contextmap);
            }                
        }

        public Jsapigeolocationmap Getmap(string id)
        {
            return this.Mapitems.FirstOrDefault(item => item.Id == id);
        }
        public void Removemap(string id)
        {
            var map = this.Getmap(id);
            if (map != null)
                this.Mapitems.Remove(map);
        }

        public async Task Getgeolocationpermissions(string id)
        {
            var map = this.Getmap(id);
            if (map != null)
                await map.Jsmapreference.InvokeVoidAsync("requestpermissions");
        }
        public async Task Getgeolocation(string id)
        {
            var map = this.Getmap(id);
            if (map != null)
                await map.Jsmapreference.InvokeVoidAsync("requestcoords");
        }
        public async Task Rendergooglemapposition(string id, double? latitude, double? longitude)
        {
            var map = this.Getmap(id);
            if (map != null)
                await map.Jsmapreference.InvokeVoidAsync("rendergooglemapposition", latitude, longitude);
        }

        [JSInvokable("Pushcoords")]
        public async void Pushcoords(string id, string coords)
        {
            var map = this.Getmap(id);
            if (map != null)
            {
                map.Item = JsonSerializer.Deserialize<Jsapigeolocationitem>(coords);
                this.UpdateUI();
            }

            //await this.Rendergooglemapposition(item.latitude, item.longitude);
            //this.UpdateUI();
        }

        [JSInvokable("Permissionschanged")]
        public void Permissionschanged(string permissionsstate)
        {
            this.OnGeolocationpermisssionsChanged?.Invoke(new Jsapigeolocationpermissionsevent() { Permissionsstate = permissionsstate });
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