using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hwavmvid.Jsapigeolocation
{

    public class Jsapigeolocationservice : IDisposable
    {
        
        public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference Module { get; set; }
        public IJSObjectReference Map { get; set; }

        public DotNetObjectReference<Jsapigeolocationservice> DotNetObjectRef;

        public event Action<Jsapigeolocationpermissionsevent> OnGeolocationDetected;
        public event Action<Jsapigeolocationpermissionsevent> OnGeolocationpermisssionsChanged;

        public event Action OnUpdateUI;

        public Jsapigeolocationitem item { get; set; } = new Jsapigeolocationitem();

        public Jsapigeolocationservice(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.DotNetObjectRef = DotNetObjectReference.Create(this);
        }
        public async Task Initgeolocationservice()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/jsapigeolocationjsinterop.js");
            }
        }
        public async Task InitGeolocationMap()
        {
            this.Map = await this.Module.InvokeAsync<IJSObjectReference>("initgeolocationmap", this.DotNetObjectRef);
        }

        public async Task Getgeolocationpermissions()
        {
            await this.Map.InvokeVoidAsync("requestpsermissions");
        }
        public async Task Getgeolocation()
        {
            await this.Map.InvokeVoidAsync("requestcoords");
        }

        [JSInvokable("Pushcoords")]
        public void Pushcoords(string coords)
        {
            this.item = JsonSerializer.Deserialize<Jsapigeolocationitem>(coords);
            this.UpdateUI();
        }

        [JSInvokable("Permissionschanged")]
        public void Permissionschanged(string permissionsstate)
        {
            this.OnGeolocationpermisssionsChanged?.Invoke(new Jsapigeolocationpermissionsevent() { Permissionsstate = permissionsstate });
        }

        public void UpdateUI()
        {
            this.OnUpdateUI.Invoke();
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