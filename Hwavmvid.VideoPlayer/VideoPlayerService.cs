using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hwavmvid.VideoPlayer
{

    public class VideoPlayerService : IAsyncDisposable
    {

        public List<VideoPlayerModel> VideoPlayerMaps { get; set; } = new List<VideoPlayerModel>();
        public IJSObjectReference Module { get; set; }
        public IJSRuntime JsRuntime { get; set; }

        public DotNetObjectReference<VideoPlayerServiceExtension> DotNetObjectRef;
        public VideoPlayerServiceExtension VideoServiceExtension;

        public event Action<string> RunUpdateUI;
        public event Action<string> OnError;

        public VideoPlayerService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.VideoServiceExtension = new VideoPlayerServiceExtension(this);
            this.DotNetObjectRef = DotNetObjectReference.Create(this.VideoServiceExtension);
        }
        public async Task InitVideoPlayer()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazorvideoplayerjsinterop.js");
            }
        }
        public async Task InitVideoMap(string mapid, string parameterId1, string parameterId2)
        {
            IJSObjectReference jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initblazorvideoplayer", this.DotNetObjectRef, mapid);
            var map = this.VideoPlayerMaps.FirstOrDefault(item => item.MapId == mapid);
            if (map == null)
            {
                this.VideoPlayerMaps.Add(
                    new VideoPlayerModel()
                    {
                        MapId = mapid,
                        ParameterId1 = parameterId1,
                        ParameterId2 = parameterId2,
                        LastSequenceId = string.Empty,
                        JsObjRef = jsobjref,
                        VideoOverlay = true,
                        SliderCurrentValue = 1,
                        SliderValueChanged = false,
                    });                
            }
            else if (map != null)
                map.JsObjRef = jsobjref;
        }

        public VideoPlayerModel GetVideoMap(string mapid)
        {
            return this.VideoPlayerMaps.FirstOrDefault(item => item.MapId == mapid);
        }

        public void RemoveVideoMap(string mapId)
        {
            var obj = this.VideoPlayerMaps.FirstOrDefault(item => item.MapId == mapId);
            if (obj != null)
            {
                this.VideoPlayerMaps.Remove(obj);
            }
        }

        public async Task GetFirstSequence(string mapid)
        {
            var obj = this.GetVideoMap(mapid);
            if (obj != null && obj.JsObjRef != null)
            {
                await obj.JsObjRef.InvokeVoidAsync("initremotelivestream");
                await obj.JsObjRef.InvokeVoidAsync("getfirstsequenceremotelivestream");
            }
        }
        public async Task StartVideo(string mapid)
        {
            var obj = this.GetVideoMap(mapid);
            if (obj != null && obj.JsObjRef != null)
            {
                obj.VideoOverlay = false;
                obj.SliderCurrentValue = 1;
                obj.SliderValueChanged = false;

                await obj.JsObjRef.InvokeVoidAsync("startremotelivestream");
            }
        }

        public async Task ClearVideoBuffer(string mapid)
        {
            var obj = this.GetVideoMap(mapid);
            if (obj != null && obj.JsObjRef != null)
            {
                await obj.JsObjRef.InvokeVoidAsync("clearvideobufferremotelivestream");
                this.RunUpdateUI?.Invoke(obj.MapId);
            }
        }
        public async Task AddVideoSequence(string mapid, VideoPlayerApiItem apiItem)
        {
            var obj = this.GetVideoMap(mapid);
            if (obj != null && obj.JsObjRef != null)
            {
                if (obj.SliderValueChanged && !apiItem.SliderValueChanged)
                {
                    return;
                }

                obj.LastSequenceId = apiItem.LastSequence;
                obj.SliderCurrentValue = apiItem.SliderCurrentValue;
                obj.SliderTotalSequences = apiItem.TotalSequences;
                obj.SliderValueChanged = false;

                await obj.JsObjRef.InvokeVoidAsync("appendbufferremotelivestream", apiItem.Base64DataUri);
                this.RunUpdateUI?.Invoke(obj.MapId);
            }
        }
        public async Task ContinueVideo(string mapid)
        {
            var obj = this.GetVideoMap(mapid);
            if (obj != null && obj.JsObjRef != null)
                await obj.JsObjRef.InvokeVoidAsync("continueremotelivestream");
        }

        public void ThrowError(string message)
        {
            this.OnError?.Invoke(message);
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var map in this.VideoPlayerMaps)
            {
                if (map.JsObjRef != null)
                {
                    await map.JsObjRef.InvokeVoidAsync("closeremotelivestream");
                    await map.JsObjRef.DisposeAsync();
                }
                    
            }
        }

    }

    public class VideoPlayerServiceExtension
    {

        public VideoPlayerService VideoPlayerService { get; set; }

        public event Action<VideoPlayerModel> OnGetNextSequence;

        public VideoPlayerServiceExtension(VideoPlayerService VideoService)
        {
            this.VideoPlayerService = VideoService;
        }

        [JSInvokable("GetNextSequence")]
        public async void GetNextSequence(string mapid)
        {
            var map = this.VideoPlayerService.GetVideoMap(mapid);
            if (map != null)
            {
                if (!map.SliderValueChanged)
                {
                    await Task.Run(() => OnGetNextSequence.Invoke(map));
                }
            }
        }
        public async void GetNextSequenceOnSliderValueChanged(string mapid)
        {
            var map = this.VideoPlayerService.GetVideoMap(mapid);
            if (map != null)
            {
                if (map.SliderValueChanged)
                {
                    await Task.Run(() => OnGetNextSequence.Invoke(map));
                }
            }
        }

    }

}
