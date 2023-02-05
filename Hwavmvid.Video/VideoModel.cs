using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hwavmvid.Video
{
    public class VideoModel
    {

        public Guid MapId { get; set; }
        public Task Streamtask { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public VideoType Type { get; set; }
        public VideoSourceType SourceType { get; set; }
        public IJSObjectReference JsObjRef { get; set; }
        public bool VideoOverlay { get; set; }
        public string AudioOuputId { get; set; }
        public string MicrophoneId { get; set; }
        public string WebCamId { get; set; }

    }
}
