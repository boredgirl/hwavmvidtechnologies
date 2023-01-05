using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorPager
{
    public class BlazorPagerService<TBlazorPagerItem>
    {

        private IJSRuntime JSRuntime { get; set; }
        private IJSObjectReference blazorPagerJsObjectReference { get; set; }
        private IJSObjectReference blazorPagerMap { get; set; }

        public event Action<List<TBlazorPagerItem>, int> OnRetrievedItems;
        public event Action<BlazorPagerEvent<TBlazorPagerItem>> OnRemoveItem;
        public event Action<int> OnUpdateContext;
        public event Action<Exception, int> OnError;

        public BlazorPagerService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
        }

        public async Task InitBlazorPagerService()
        {
            this.blazorPagerJsObjectReference = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazorpagerjsinterop.js");
            this.blazorPagerMap = await this.blazorPagerJsObjectReference.InvokeAsync<IJSObjectReference>("initblazorpager");
        }
        public void ExposeItems(List<TBlazorPagerItem> items, int apiqueryid)
        {
            this.OnRetrievedItems?.Invoke(items, apiqueryid);
        }
        public async Task ScrollTop(string elementId)
        {
            if(this.blazorPagerMap != null)
            {
                await this.blazorPagerMap.InvokeVoidAsync("scrollToElement", elementId);
            }
        }
        public void RemoveItem(TBlazorPagerItem item, int apiQueryId)
        {
            this.OnRemoveItem?.Invoke(new BlazorPagerEvent<TBlazorPagerItem>() { Item = item, ApiQueryId = apiQueryId });
        }
        public void UpdateContext(int apiQueryId)
        {
            this.OnUpdateContext?.Invoke(apiQueryId);
        }
        public void ThrowError(Exception exception, int apiQueryId)
        {
            this.OnError?.Invoke(exception, apiQueryId);
        }

    }
}
