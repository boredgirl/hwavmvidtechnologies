using Microsoft.JSInterop;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorDownload
{
    public class BlazorDownloadMap
    {

        public string Id { get; set; }
        public string ApiQueryId { get; set; }
        public string FileType { get; set; }
        public IJSObjectReference JsObjectReference { get; set; }
        public bool DownloadInProgress { get; set; }
        public Task DownloadTask { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public BlazorDownloadApiItem LastApiItem { get; set; }

    }
}
