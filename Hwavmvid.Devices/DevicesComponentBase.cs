using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hwavmvid.Devices
{
    public class DevicesComponentBase : ComponentBase, IDisposable
    {

        [Inject] public DevicesService DevicesServices { get; set; }

        [Parameter] public string Id { get; set; }
        [Parameter] public DeviceItem AudioDefaultDevice { get; set; }
        [Parameter] public DeviceItem MicrophoneDefaultDevice { get; set; }
        [Parameter] public DeviceItem WebcamDefaultDevice { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.DevicesServices.OnUpdateUI += UpdateUI;
            await base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.DevicesServices.InitDevices();
                await this.DevicesServices.InitDevicesMap();
                await this.DevicesServices.GetDevices();

                var audioItem = this.DevicesServices.item.audios.FirstOrDefault(item => item.id == this.AudioDefaultDevice.id);
                this.AudioDefaultDevice = audioItem ?? this.DevicesServices.item.audios.FirstOrDefault();
                
                var microphoneItem = this.DevicesServices.item.microphones.FirstOrDefault(item => item.id == this.MicrophoneDefaultDevice.id);
                this.MicrophoneDefaultDevice = microphoneItem ?? this.DevicesServices.item.microphones.FirstOrDefault();
                
                var webcamItem = this.DevicesServices.item.webcams.FirstOrDefault(item => item.id == this.WebcamDefaultDevice.id);
                this.WebcamDefaultDevice = webcamItem ?? this.DevicesServices.item.webcams.FirstOrDefault();

                this.UpdateUI();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void SetAudioDefaultDevice(DeviceItem item)
        {
            this.AudioDefaultDevice = item;
            this.DevicesServices.AudioDefaultDeviceChanged(this.Id, item);
            this.UpdateUI();
        }
        public void SetMicrophoneDefaultDevice(DeviceItem item)
        {
            this.MicrophoneDefaultDevice = item;
            this.DevicesServices.MicrophoneDefaultDeviceChanged(this.Id, item);
            this.UpdateUI();
        }
        public void SetWebcamDefaultDevice(DeviceItem item)
        {
            this.WebcamDefaultDevice = item;
            this.DevicesServices.WebcamDefaultDeviceChanged(this.Id, item);
            this.UpdateUI();
        }

        private async void UpdateUI()
        {
            await InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.DevicesServices.OnUpdateUI -= UpdateUI;
        }

    }
}
