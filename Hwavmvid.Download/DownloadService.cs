using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Json;

namespace Hwavmvid.Download
{
    public class DownloadService : IDisposable
    {

        private IJSRuntime JsRuntime { get; set; }
        private IJSObjectReference JsImport { get; set; }
        private HttpClient HttpClient { get; set; }
        private List<DownloadMap> Maps { get; set; } = new List<DownloadMap>();

        public HubConnection HubConnection { get; set; }
        public string HubConnectionMethodName { get; set; }
        public event Action<DownloadEvent> OnApiItemReceived;
        public event Action<DownloadApiItem> OnUpdateProgress;

        public DownloadService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            this.JsRuntime = jsRuntime;
            this.HttpClient = httpClient;
            this.OnUpdateProgress += UpdateProgress;
        }

        public async Task InitDownload(string id, string apiQueryId, string fileType, HubConnection hubConnection, string hubContextMethodName)
        {
            if (this.JsImport == null)
            {
                this.JsImport = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/downloadjsinterop.js");
            }

            if (this.JsImport != null)
            {
                var jsMap = await this.JsImport.InvokeAsync<IJSObjectReference>("initdownload");
                DownloadMap map = this.GetMap(id);
                if (map == null)
                {
                    var item = new DownloadMap()
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

        public DownloadMap GetMap(string id)
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
                var apiItem = await getItemsResponse.Content.ReadFromJsonAsync<DownloadApiItem>();

                this.DownloadCompleted(id, apiItem);
                this.UpdateProgress(apiItem);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void DownloadCompleted(string id, DownloadApiItem item)
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
            this.HubConnection.On(hubConnectionMethod, (DownloadApiItem apiItem) => this.OnUpdateProgress(apiItem));
        }

        public void RemoveHubConnectionHandler(string hubConnectionMethod)
        {
            this.HubConnection.Remove(hubConnectionMethod);
        }

        public void UpdateProgress(DownloadApiItem apiItem)
        {
            var eventItem = new DownloadEvent()
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