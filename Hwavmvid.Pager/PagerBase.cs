using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Hwavmvid.Pager
{
    public partial class PagerBase<TPagerItem> : ComponentBase, IDisposable
    {

        [Inject] protected HttpClient HttpClient { get; set; }
        [Inject] protected PagerService<TPagerItem> PagerService { get; set; }

        [Parameter] public RenderFragment<TPagerItem> PagerItem { get; set; }
        [Parameter] public List<TPagerItem> ContextPageItems { get; set; }
        [Parameter] public string Class { get; set; }
        [Parameter] public string ElementId { get; set; }
        [Parameter] public string GetItemsApiUrl { get; set; }
        [Parameter] public string HubConnectionId { get; set; }
        [Parameter] public int ApiQueryId { get; set; }
        [Parameter] public int ItemsPerPage { get; set; }
        [Parameter] public bool Scrolling { get; set; }

        public int ContextPage { get; set; } = 1;
        public int PagesTotal { get; set; } = -1;
        public bool Loading { get; set; }

        private string requestUri 
        {   
            get
            {
                var uri = this.GetItemsApiUrl + "/" + this.ContextPage.ToString() + "/" + this.ItemsPerPage.ToString() + "/" + this.ApiQueryId.ToString();

                if (!string.IsNullOrEmpty(this.HubConnectionId))
                    uri += "/" + this.HubConnectionId;

                return uri;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            this.PagerService.OnRemoveItem += OnRemoveItem;
            this.PagerService.OnUpdateContext += (int apiQueryId) => UpdateContextComponent(apiQueryId);

            await this.UpdateContextAsync();
            await base.OnInitializedAsync();
        }

        private async void UpdateContextComponent(int obj)
        {
            if (this.ApiQueryId == obj)
            {
                await this.UpdateContextAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                await this.PagerService.InitPagerService();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public async Task UpdateContextAsync()
        {
            try
            {
                if (this.Loading)
                    return;

                this.Loading = true;
                this.ContextPageItems.Clear();

                var getItemsResponse = await this.HttpClient.GetAsync(this.requestUri);
                var apiItem = await getItemsResponse.Content.ReadFromJsonAsync<PagerApiItem<TPagerItem>>();

                this.PagerService.ExposeItems(apiItem.Items, this.ApiQueryId);
                this.PagesTotal = apiItem.Pages;

                this.Loading = false;
                await InvokeAsync(() =>
                {
                    this.StateHasChanged();
                });
                
                if (this.Scrolling)
                    await this.PagerService.ScrollTop(this.ElementId);
                
            }
            catch (Exception exception)
            {
                this.PagerService.ThrowError(exception, this.ApiQueryId);
            }
        }
        public async Task SetContextPageAsync(int index)
        {
            this.ContextPage = index;
            await this.UpdateContextAsync();
        }
        public async Task LoadMore_Clicked()
        {
            try
            {
                if (this.Loading)
                    return;

                this.Loading = true;
                this.ContextPage++;

                var getItemsResponse = await this.HttpClient.GetAsync(this.requestUri);
                var apiItem = await getItemsResponse.Content.ReadFromJsonAsync<PagerApiItem<TPagerItem>>();

                this.PagerService.ExposeItems(apiItem.Items, this.ApiQueryId);
                this.PagesTotal = apiItem.Pages;

                this.Loading = false;
                await InvokeAsync(() =>
                {
                    this.StateHasChanged();
                });
            }
            catch (Exception exception)
            {
                this.PagerService.ThrowError(exception, this.ApiQueryId);
            }
        }

        public async Task NextAsync()
        {
            this.ContextPage++;
            await this.UpdateContextAsync();
        }
        public async Task PreviousAsync()
        {
            this.ContextPage--;
            await this.UpdateContextAsync();
        }
        public async Task FirstAsync()
        {
            this.ContextPage = 1;
            await this.UpdateContextAsync();
        }
        public async Task LastAsync()
        {
            this.ContextPage = this.PagesTotal;
            await this.UpdateContextAsync();
        }

        private async void OnRemoveItem(PagerEvent<TPagerItem> obj)
        {
            await Task.Delay(200).ContinueWith(async (task) =>
            {
                await this.UpdateContextAsync();
            });
        }

        public void Dispose()
        {
            this.PagerService.OnRemoveItem -= OnRemoveItem;
            this.PagerService.OnUpdateContext -= (int apiQueryId) => UpdateContextComponent(apiQueryId);
        }

    }
}
