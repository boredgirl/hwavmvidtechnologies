using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorDevices
{

    public class BlazorDevicesService : IDisposable
    {
        
        public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference Module { get; set; }
        public IJSObjectReference Map { get; set; }

        public DotNetObjectReference<BlazorDevicesService> DotNetObjectRef;

        public event Action<BlazorDevicesEvent> OnAudioDeviceChanged;
        public event Action<BlazorDevicesEvent> OnMicrophoneDeviceChanged;
        public event Action<BlazorDevicesEvent> OnWebcamDeviceChanged;
        public event Action OnUpdateUI;

        public BlazorDevicesItem item { get; set; } = new BlazorDevicesItem();

        public BlazorDevicesService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.DotNetObjectRef = DotNetObjectReference.Create(this);
        }
        public async Task InitBlazorDevices()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazordevicesjsinterop.js");
            }
        }
        public async Task InitBlazorDevicesMap()
        {
            this.Map = await this.Module.InvokeAsync<IJSObjectReference>("initblazordevices", this.DotNetObjectRef);
        }

        public async Task GetDevices()
        {
            await this.Map.InvokeVoidAsync("getitems");
        }
        public async Task SetDevices()
        {
            await this.Map.InvokeVoidAsync("setitems");
        }

        [JSInvokable("AddAudios")]
        public void AddAudio(string audios)
        {
            this.item.audios = JsonSerializer.Deserialize<BlazorDeviceItem[]>(audios).ToList();
            this.UpdateUI();
        }
        [JSInvokable("AddMicrophones")]
        public void AddMicrophone(string microphones)
        {
            this.item.microphones = JsonSerializer.Deserialize<BlazorDeviceItem[]>(microphones).ToList();
            this.UpdateUI();
        }
        [JSInvokable("AddWebcams")]
        public void AddWebcam(string webcams)
        {
            this.item.webcams = JsonSerializer.Deserialize<BlazorDeviceItem[]>(webcams).ToList();
            this.UpdateUI();
        }

        public void AudioDefaultDeviceChanged(string id, BlazorDeviceItem item)
        {
            this.OnAudioDeviceChanged?.Invoke(new BlazorDevicesEvent() { Id = id, Item = item });
        }
        public void MicrophoneDefaultDeviceChanged(string id, BlazorDeviceItem item)
        {
            this.OnMicrophoneDeviceChanged?.Invoke(new BlazorDevicesEvent() { Id = id, Item = item });
        }
        public void WebcamDefaultDeviceChanged(string id, BlazorDeviceItem item)
        {
            this.OnWebcamDeviceChanged?.Invoke(new BlazorDevicesEvent() { Id = id, Item = item });
        }

        public void UpdateUI()
        {
            this.OnUpdateUI.Invoke();
        }

        public void Dispose()
        {
            if (this.Module != null)
            {
                this.Module.DisposeAsync();
            }
        }

    }
}