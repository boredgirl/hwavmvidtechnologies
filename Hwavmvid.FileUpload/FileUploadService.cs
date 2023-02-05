using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Hwavmvid.FileUpload
{
    public class FileUploadService
    {

        public IJSRuntime JSRuntime;
        public IJSObjectReference Module;
        public IJSObjectReference FileUploadMap;

        public FileUploadServiceExtension FileUploadServiceExtension;

        public DotNetObjectReference<FileUploadServiceExtension> dotNetObjectReference;

        public FileUploadService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
            this.FileUploadServiceExtension = new FileUploadServiceExtension();
            this.dotNetObjectReference = DotNetObjectReference.Create(this.FileUploadServiceExtension);
        }

        public async Task InitFileUploadDropzone(string inputFileId, string elementId)
        {
            this.Module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazorfileuploadjsinterop.js");
            this.FileUploadMap = await this.Module.InvokeAsync<IJSObjectReference>("initfileupload", this.dotNetObjectReference, inputFileId, elementId);
        }

    }

    public class FileUploadServiceExtension
    {

        public event EventHandler<FileUploadEvent> OnDropEvent;

        [JSInvokable("OnDrop")]
        public void OnDrop(string elementId)
        {
            FileUploadEvent eventParameters = new FileUploadEvent() { FileUploadDropzoneId = elementId };
            OnDropEvent?.Invoke(this, eventParameters);
        }

    }
}
