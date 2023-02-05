using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hwavmvid.Pager
{
    public class PagerService<TPagerItem>
    {

        private IJSRuntime JSRuntime { get; set; }
        private IJSObjectReference pagerJsObjectReference { get; set; }
        private IJSObjectReference pagerMap { get; set; }

        public event Action<List<TPagerItem>, int> OnRetrievedItems;
        public event Action<PagerEvent<TPagerItem>> OnRemoveItem;
        public event Action<int> OnUpdateContext;
        public event Action<Exception, int> OnError;

        public PagerService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
        }

        public async Task InitPagerService()
        {
            this.pagerJsObjectReference = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazorpagerjsinterop.js");
            this.pagerMap = await this.pagerJsObjectReference.InvokeAsync<IJSObjectReference>("initpager");
        }
        public void ExposeItems(List<TPagerItem> items, int apiqueryid)
        {
            this.OnRetrievedItems?.Invoke(items, apiqueryid);
        }
        public async Task ScrollTop(string elementId)
        {
            if(this.pagerMap != null)
            {
                await this.pagerMap.InvokeVoidAsync("scrollToElement", elementId);
            }
        }
        public void RemoveItem(TPagerItem item, int apiQueryId)
        {
            this.OnRemoveItem?.Invoke(new PagerEvent<TPagerItem>() { Item = item, ApiQueryId = apiQueryId });
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
