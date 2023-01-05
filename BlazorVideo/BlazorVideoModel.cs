using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoModel
    {

        public Guid MapId { get; set; }
        public Task Streamtask { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public BlazorVideoType Type { get; set; }
        public BlazorVideoSourceType SourceType { get; set; }
        public IJSObjectReference JsObjRef { get; set; }
        public bool VideoOverlay { get; set; }
        public string AudioOuputId { get; set; }
        public string MicrophoneId { get; set; }
        public string WebCamId { get; set; }

    }
}
