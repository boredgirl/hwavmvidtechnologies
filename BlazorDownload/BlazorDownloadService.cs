using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorDownload
{
    public class BlazorDownloadService : IDisposable
    {

        private IJSRuntime JsRuntime { get; set; }
        private IJSObjectReference JsImport { get; set; }
        private HttpClient HttpClient { get; set; }
        private List<BlazorDownloadMap> Maps { get; set; } = new List<BlazorDownloadMap>();

        public HubConnection HubConnection { get; set; }
        public string HubConnectionMethodName { get; set; }
        public event Action<BlazorDownloadEvent> OnApiItemReceived;
        public event Action<BlazorDownloadApiItem> OnUpdateProgress;

        public BlazorDownloadService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            this.JsRuntime = jsRuntime;
            this.HttpClient = httpClient;
            this.OnUpdateProgress += UpdateProgress;
        }

        public async Task InitBlazorDownload(string id, string apiQueryId, string fileType, HubConnection hubConnection, string hubContextMethodName)
        {
            if (this.JsImport == null)
            {
                this.JsImport = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazordownloadjsinterop.js");
            }

            if (this.JsImport != null)
            {
                var jsMap = await this.JsImport.InvokeAsync<IJSObjectReference>("initblazordownload");
                BlazorDownloadMap map = this.GetMap(id);
                if (map == null)
                {
                    var item = new BlazorDownloadMap()
                    {
                        Id = id,
                        ApiQueryId = apiQueryId,
                        FileType = fileType,
                        JsObjectReference = jsMap,
                    };

                    this.Maps.Add(item);
                }
                else
                    map.JsObjectReference = jsMap;
            }

            this.HubConnection = hubConnection;
            this.HubConnectionMethodName = hubContextMethodName;
            this.RegisterHubConnectionHandler(hubContextMethodName);
        }

        public BlazorDownloadMap GetMap(string id)
        {
            return this.Maps.FirstOrDefault(item => item.Id == id);
        }

        public void StartDownloadFile(string id, string requestUri)
        {
            
            var map = this.GetMap(id);
            if (map != null && !map.DownloadInProgress)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                Task task = new Task(async () => await this.DownloadTaskImplementation(id, requestUri, token));

                map.DownloadInProgress = true;
                map.DownloadTask = task;
                map.CancellationTokenSource = tokenSource;

                task.Start();
            }
        }

        public async Task DownloadTaskImplementation(string id, string requestUri, CancellationToken token)
        {
            try
            {
                var getItemsResponse = await this.HttpClient.GetAsync(requestUri);
                var apiItem = await getItemsResponse.Content.ReadFromJsonAsync<BlazorDownloadApiItem>();

                this.DownloadCompleted(id, apiItem);
                this.UpdateProgress(apiItem);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void DownloadCompleted(string id, BlazorDownloadApiItem item)
        {
            
            var map = this.GetMap(id);
            if (map != null)
            {
                var fileName = Guid.NewGuid().ToString() + map.FileType;

                map.CancellationTokenSource.Cancel();
                map.DownloadTask.Dispose();
                map.DownloadInProgress = false;

                if (map.JsObjectReference != null)
                    map.JsObjectReference.InvokeVoidAsync("downloadcapturedvideoitem", fileName, item.Base64Uri);

            }
        }

        public void RegisterHubConnectionHandler(string hubConnectionMethod)
        {
            this.HubConnection.On(hubConnectionMethod, (BlazorDownloadApiItem apiItem) => this.OnUpdateProgress(apiItem));
        }

        public void RemoveHubConnectionHandler(string hubConnectionMethod)
        {
            this.HubConnection.Remove(hubConnectionMethod);
        }

        public void UpdateProgress(BlazorDownloadApiItem apiItem)
        {
            var eventItem = new BlazorDownloadEvent()
            {
                ApiItem = apiItem,
            };

            this.OnApiItemReceived?.Invoke(eventItem);
        }

        public void Dispose()
        {
            foreach (var map in this.Maps)
            {
                map.JsObjectReference.DisposeAsync();
            }

            this.RemoveHubConnectionHandler(this.HubConnectionMethodName);
            this.OnUpdateProgress -= UpdateProgress;
        }

    }
}