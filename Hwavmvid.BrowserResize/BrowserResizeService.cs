using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Hwavmvid.BrowserResize
{
    public class BrowserResizeService
    {

        public IJSRuntime JsRuntime;
        public IJSObjectReference Module;
        public IJSObjectReference BrowserResizeMap;
        public DotNetObjectReference<BrowserResizeServiceExtension> DotNetObjRef;
        public BrowserResizeServiceExtension BrowserResizeServiceExtension;

        public BrowserResizeService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.BrowserResizeServiceExtension = new BrowserResizeServiceExtension();
            this.DotNetObjRef = DotNetObjectReference.Create(this.BrowserResizeServiceExtension);
        }
        public async Task InitBrowserResizeService()
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/browserresizejsinterop.js");
            this.BrowserResizeMap = await this.Module.InvokeAsync<IJSObjectReference>("initbrowserresize", this.DotNetObjRef);
        }

        public async Task RegisterWindowResizeCallback()
        {
            await this.BrowserResizeMap.InvokeVoidAsync("registerResizeCallback");
        }
        public async Task<int> GetInnerHeight()
        {
            return await this.BrowserResizeMap.InvokeAsync<int>("getInnerHeight");
        }
        public async Task<int> GetInnerWidth()
        {
            return await this.BrowserResizeMap.InvokeAsync<int>("getInnerWidth");
        }

    }

    public class BrowserResizeServiceExtension
    {

        public event Func<Task> OnResize;

        [JSInvokable("OnBrowserResize")]
        public async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }

    }
}
