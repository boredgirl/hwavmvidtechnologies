using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hwavmvid.Devices
{

    public class DevicesService : IDisposable
    {
        
        public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference Module { get; set; }
        public IJSObjectReference Map { get; set; }

        public DotNetObjectReference<DevicesService> DotNetObjectRef;

        public event Action<DevicesEvent> OnAudioDeviceChanged;
        public event Action<DevicesEvent> OnMicrophoneDeviceChanged;
        public event Action<DevicesEvent> OnWebcamDeviceChanged;
        public event Action OnUpdateUI;

        public DevicesItem item { get; set; } = new DevicesItem();

        public DevicesService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.DotNetObjectRef = DotNetObjectReference.Create(this);
        }
        public async Task InitDevices()
        {
            if (this.Module == null)
            {
                this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazordevicesjsinterop.js");
            }
        }
        public async Task InitDevicesMap()
        {
            this.Map = await this.Module.InvokeAsync<IJSObjectReference>("initdevices", this.DotNetObjectRef);
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
            this.item.audios = JsonSerializer.Deserialize<DeviceItem[]>(audios).ToList();
            this.UpdateUI();
        }
        [JSInvokable("AddMicrophones")]
        public void AddMicrophone(string microphones)
        {
            this.item.microphones = JsonSerializer.Deserialize<DeviceItem[]>(microphones).ToList();
            this.UpdateUI();
        }
        [JSInvokable("AddWebcams")]
        public void AddWebcam(string webcams)
        {
            this.item.webcams = JsonSerializer.Deserialize<DeviceItem[]>(webcams).ToList();
            this.UpdateUI();
        }

        public void AudioDefaultDeviceChanged(string id, DeviceItem item)
        {
            this.OnAudioDeviceChanged?.Invoke(new DevicesEvent() { Id = id, Item = item });
        }
        public void MicrophoneDefaultDeviceChanged(string id, DeviceItem item)
        {
            this.OnMicrophoneDeviceChanged?.Invoke(new DevicesEvent() { Id = id, Item = item });
        }
        public void WebcamDefaultDeviceChanged(string id, DeviceItem item)
        {
            this.OnWebcamDeviceChanged?.Invoke(new DevicesEvent() { Id = id, Item = item });
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