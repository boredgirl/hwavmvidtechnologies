using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hwavmvid.Video
{

    public class VideoService : IAsyncDisposable
    {

        public List<VideoModel> VideoMaps { get; set; } = new List<VideoModel>();
        public IJSObjectReference Module { get; set; }
        public IJSRuntime JsRuntime { get; set; }

        public DotNetObjectReference<VideoServiceExtension> DotNetObjectRef;
        public VideoServiceExtension VideoServiceExtension;

        public event Action<VideoModel> StartVideoEvent;
        public event Action<VideoModel> StopVideoEvent;
        public event Action<string, string, string, VideoSnapshotActivatorType> TookSnapshotEvent;
        public event Action<string> OnError;
        public event Action<string, string> RunUpdateUI;

        public List<VideoModel> LocalStreamTasks { get; set; } = new List<VideoModel>();
        public List<VideoModel> RemoteStreamTasks { get; set; } = new List<VideoModel>();

        private int VideoSegmentsLength { get; set; }

        public VideoService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.VideoServiceExtension = new VideoServiceExtension(this);
            this.DotNetObjectRef = DotNetObjectReference.Create(this.VideoServiceExtension);
        }
        public async Task InitVideo()
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazorvideojsinterop.js");
        }
        public async Task InitVideoMap(string id1, string id2, VideoType type, VideoSourceType sourceType, int framerate, int videoBitsPerSecond, int audioBitsPerSecond, int videoSegmentsLength, string audioDefaultDeviceId, string microphoneDefaultDeviceId, string webcamDefaultDeviceId)
        {
            this.VideoSegmentsLength = videoSegmentsLength;
            var map = this.GetVideoMap(id1, id2);
            if (map == null)
            {
                IJSObjectReference jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initblazorvideo", this.DotNetObjectRef, id1, id2, type.ToString().ToLower(), sourceType.ToString().ToLower(), framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);
                this.AddVideoMap(id1, id2, type, sourceType, jsobjref);
            }
        }

        public void AddVideoMap(string id1, string id2, VideoType type, VideoSourceType sourceType, IJSObjectReference jsobjref)
        {
            if (!this.VideoMaps.Any(item => item.Id1 == id1 && item.Id2 == id2))
            {
                this.VideoMaps.Add(new VideoModel() { MapId = Guid.NewGuid(), Id1 = id1, Id2 = id2, Type = type, SourceType = sourceType, JsObjRef = jsobjref, VideoOverlay = true });
            }
        }
        public void RemoveVideoMap(Guid guid)
        {
            var obj = this.VideoMaps.FirstOrDefault(item => item.MapId == guid);
            if (obj != null)
            {
                this.VideoMaps.Remove(obj);
            }
        }
        public VideoModel GetVideoMap(string id1, string id2)
        {
            return this.VideoMaps.FirstOrDefault(item => item.Id1 == id1 && item.Id2 == id2);
        }

        public async Task InitLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                switch (obj.SourceType)
                {
                    case VideoSourceType.Webcams:
                        await obj.JsObjRef.InvokeVoidAsync("initlocallivestreamwebcams", obj.AudioOuputId, obj.MicrophoneId, obj.WebCamId);
                        break;

                    case VideoSourceType.Websource:
                        await obj.JsObjRef.InvokeVoidAsync("initlocallivestreamwebsource");
                        break;

                    case VideoSourceType.Webscreen:
                        await obj.JsObjRef.InvokeVoidAsync("initlocallivestreamwebscreen");
                        break;
                };
            }
        }
        public async Task InitDevicesLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                if (obj.SourceType == VideoSourceType.Webcams)
                {
                    await obj.JsObjRef.InvokeVoidAsync("initdeviceslocallivestreamwebcams");
                }
            }
        }
        public async Task StartBroadcastingLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                await obj.JsObjRef.InvokeVoidAsync("startbroadcastinglocallivestreamwebcams");
            }
        }
        public async Task StartVideoSource(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                await obj.JsObjRef.InvokeVoidAsync("startvideolocallivestreamwebsource");
            }
        }
        public async Task StartVideoScreen(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                await obj.JsObjRef.InvokeVoidAsync("startvideolocallivestreamwebscreen");
            }
        }
        public async Task StartSequenceLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                if (obj.SourceType == VideoSourceType.Webcams)
                    await obj.JsObjRef.InvokeVoidAsync("startsequencelocallivestreamwebcams");

                else if (obj.SourceType == VideoSourceType.Websource)
                    await obj.JsObjRef.InvokeVoidAsync("startsequencelocallivestreamwebsource");

                else if (obj.SourceType == VideoSourceType.Webscreen)
                    await obj.JsObjRef.InvokeVoidAsync("startsequencelocallivestreamwebscreen");
            }
        }
        public async Task StopSequenceLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                if (obj.SourceType == VideoSourceType.Webcams)
                    await obj.JsObjRef.InvokeVoidAsync("stopsequencelocallivestreamwebcams");

                else if (obj.SourceType == VideoSourceType.Websource)
                    await obj.JsObjRef.InvokeVoidAsync("stopsequencelocallivestreamwebsource");

                else if (obj.SourceType == VideoSourceType.Webscreen)
                    await obj.JsObjRef.InvokeVoidAsync("stopsequencelocallivestreamwebscreen");
            }
        }
        public async Task ContinueLocalLivestreamAsync(string id1, string id2)
        {
            List<VideoModel> localList = this.LocalStreamTasks.Where(item => item.Id1 == id1 && item.Id2 == id2).ToList();
            List<VideoModel> remoteList = this.RemoteStreamTasks.Where(item => item.Id1 == id1 && item.Id2 == id2).ToList();

            if (localList.Any() || remoteList.Any())
            {
                await this.StartVideoChat(id1, id2);
            }
        }
        public async Task CloseLocalLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                if (obj.SourceType == VideoSourceType.Webcams)
                    await obj.JsObjRef.InvokeVoidAsync("closelocallivestreamwebcams");

                if (obj.SourceType == VideoSourceType.Websource)
                    await obj.JsObjRef.InvokeVoidAsync("closelocallivestreamwebsource");

                if (obj.SourceType == VideoSourceType.Webscreen)
                    await obj.JsObjRef.InvokeVoidAsync("closelocallivestreamwebscreen");
            }
        }

        public async Task InitRemoteLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
                await obj.JsObjRef.InvokeVoidAsync("initremotelivestream");
        }
        public async Task AppendBufferRemoteLivestream(string dataURI, string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
                await obj.JsObjRef.InvokeVoidAsync("appendbufferremotelivestream", dataURI);
        }
        public async Task CloseRemoteLivestream(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
                await obj.JsObjRef.InvokeVoidAsync("closeremotelivestream");
        }

        public async Task StartVideoChat(string id1, string id2)
        {

            try
            {
                var obj = this.GetVideoMap(id1, id2);
                if (obj != null)
                {
                    await this.StopVideoChat(id1, id2);

                    if (obj.Type == VideoType.LocalLivestream)
                    {

                        await this.InitLocalLivestream(obj.Id1, obj.Id2);

                        if (obj.SourceType == VideoSourceType.Webcams)
                        {
                            await this.InitDevicesLocalLivestream(obj.Id1, obj.Id2);
                            await this.StartBroadcastingLocalLivestream(obj.Id1, obj.Id2);
                        }

                        if (obj.SourceType == VideoSourceType.Websource)
                            await this.StartVideoSource(obj.Id1, obj.Id2);

                        if (obj.SourceType == VideoSourceType.Webscreen)
                            await this.StartVideoScreen(obj.Id1, obj.Id2);

                        obj.VideoOverlay = false;

                        CancellationTokenSource tokenSource = new CancellationTokenSource();
                        CancellationToken token = tokenSource.Token;
                        Task task = new Task(async () => await this.StreamTaskImplementation(id1, id2, token), token);
                        this.AddLocalStreamTask(obj.Id1, obj.Id2, task, tokenSource);
                        task.Start();

                        this.StartVideoEvent?.Invoke(obj);
                    }
                    else if (obj.Type == VideoType.RemoteLivestream)
                    {
                        await this.InitRemoteLivestream(obj.Id1, obj.Id2);
                        this.AddRemoteStreamTask(obj.Id1, obj.Id2);

                        obj.VideoOverlay = false;
                        this.StartVideoEvent?.Invoke(obj);
                    }
                }
            }
            catch (Exception exception)
            {
                this.ThrowError(exception.Message);
            }
        }
        public async Task StopVideoChat(string id1, string id2)
        {
            try
            {
                var obj = this.GetVideoMap(id1, id2);
                if (obj != null)
                {
                    if (obj.Type == VideoType.LocalLivestream)
                    {
                        obj.VideoOverlay = true;

                        this.RemoveLocalStreamTask(obj.Id1, obj.Id2);
                        await this.CloseLocalLivestream(obj.Id1, obj.Id2);

                        this.StopVideoEvent?.Invoke(obj);
                    }
                    else if (obj.Type == VideoType.RemoteLivestream)
                    {
                        obj.VideoOverlay = true;

                        this.RemoveRemoteStreamTask(obj.Id1, obj.Id2);
                        await this.CloseRemoteLivestream(obj.Id1, obj.Id2);

                        this.StopVideoEvent?.Invoke(obj);
                    }
                }
            }
            catch (Exception exception)
            {
                this.ThrowError(exception.Message);
            }
        }
        public async Task RestartStreamTaskIfExists(string id1, string id2)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null && obj.VideoOverlay == false)
            {
                await this.StartVideoChat(obj.Id1, obj.Id2);
            }
        }
        public void AddLocalStreamTask(string id1, string id2, Task task, CancellationTokenSource tokenSource)
        {
            this.RemoveLocalStreamTask(id1, id2);
            VideoModel obj = new VideoModel { MapId = Guid.NewGuid(), Id1 = id1, Id2 = id2, Streamtask = task, TokenSource = tokenSource };
            this.LocalStreamTasks.Add(obj);
        }
        public void RemoveLocalStreamTask(string id1, string id2)
        {
            VideoModel obj = this.LocalStreamTasks.FirstOrDefault(item => item.Id1 == id1 && item.Id2 == id2);
            if (obj != null)
            {
                try
                {
                    if (!obj.Streamtask.IsCanceled)
                    {
                        obj.TokenSource?.Cancel();
                        obj.Streamtask?.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    this.ThrowError(exception.Message);
                }

                this.LocalStreamTasks.Remove(obj);
            }
        }
        public void AddRemoteStreamTask(string id1, string id2)
        {
            var items = this.RemoteStreamTasks.Where(item => item.Id1 == id1 && item.Id2 == id2);
            if (!items.Any())
            {
                this.RemoteStreamTasks.Add(new VideoModel { MapId = Guid.NewGuid(), Id1 = id1, Id2 = id2 });
            }
        }
        public void RemoveRemoteStreamTask(string id1, string id2)
        {
            var item = this.RemoteStreamTasks.FirstOrDefault(item => item.Id1 == id1 && item.Id2 == id2);
            if (item != null)
            {
                this.RemoteStreamTasks.Remove(item);
            }
        }
        public async Task StreamTaskImplementation(string id1, string id2, CancellationToken token)
        {
            long i = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await this.StopSequenceLocalLivestream(id1, id2);
                    await this.StartSequenceLocalLivestream(id1, id2);

                    if (i % 10 == 2)
                        this.TakeSnapshot(id1, id2, VideoSnapshotActivatorType.System);
                    i++;

                    await Task.Delay(this.VideoSegmentsLength);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        public async Task TakeSnapshot(string id1, string id2, VideoSnapshotActivatorType snapshotActivatorType)
        {
            var obj = this.GetVideoMap(id1, id2);
            if (obj != null)
            {
                if (obj.Type == VideoType.LocalLivestream)
                {
                    string imageURI = string.Empty;

                    if (obj.SourceType == VideoSourceType.Webcams)
                        imageURI = await obj.JsObjRef.InvokeAsync<string>("takesnapshotlocallivestreamwebcams");

                    if (obj.SourceType == VideoSourceType.Webscreen)
                        imageURI = await obj.JsObjRef.InvokeAsync<string>("takesnapshotlocallivestreamwebscreen");

                    if (obj.SourceType == VideoSourceType.Websource)
                        imageURI = await obj.JsObjRef.InvokeAsync<string>("takesnapshotlocallivestreamwebsource");

                    if (!string.IsNullOrEmpty(imageURI))
                        this.TookSnapshotEvent.Invoke(imageURI, id1, id2, snapshotActivatorType);
                }
                else if (obj.Type == VideoType.RemoteLivestream)
                {
                    var imageURI = await obj.JsObjRef.InvokeAsync<string>("takesnapshotremotelivestream");
                    this.TookSnapshotEvent.Invoke(imageURI, id1, id2, snapshotActivatorType);
                }
            }
        }

        public void ThrowError(string message)
        {
            this.OnError?.Invoke(message);
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var task in LocalStreamTasks)
            {
                await this.StopVideoChat(task.Id1, task.Id2);
            }

            foreach (var keyvaluepair in this.VideoMaps)
            {
                if (keyvaluepair.JsObjRef != null)
                    await keyvaluepair.JsObjRef.DisposeAsync();
            }
        }

    }

    public class VideoServiceExtension
    {

        public VideoService VideoService { get; set; }
        public event Action<string, string, string> OnDataAvailableEventHandler;

        public VideoServiceExtension(VideoService VideoService)
        {
            this.VideoService = VideoService;
        }

        [JSInvokable("OnDataAvailable")]
        public void OnDataAvailable(string dataURI, string id1, string id2)
        {
            if (!string.IsNullOrEmpty(dataURI) && !string.IsNullOrEmpty(id1) && !string.IsNullOrEmpty(id2))
            {
                this.OnDataAvailableEventHandler.Invoke(dataURI, id1, id2);
            }
        }

        [JSInvokable("PauseLivestreamTask")]
        public void PauseLivestreamTask(string id1, string id2)
        {
            VideoModel obj = this.VideoService.LocalStreamTasks.FirstOrDefault(item => item.Id1 == id1 && item.Id2 == id2);
            if (obj != null)
            {
                obj.TokenSource.Cancel();
                obj.Streamtask.Dispose();
            }
        }

        [JSInvokable("OnUpdateDevices")]
        public void OnUpdateDevices(string id1, string id2, string audio, string micro, string video)
        {
            VideoModel obj = this.VideoService.GetVideoMap(id1, id2);
            if (obj != null)
            {
                obj.AudioOuputId = audio;
                obj.MicrophoneId = micro;
                obj.WebCamId = video;
            }
        }

        [JSInvokable("OnError")]
        public void OnError(string id1, string id2, string message)
        {
            VideoModel obj = this.VideoService.GetVideoMap(id1, id2);
            if (obj != null)
            {
                this.VideoService.ThrowError(message);
            }
        }

    }

}
