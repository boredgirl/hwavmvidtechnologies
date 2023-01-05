using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDevices
{
    public class BlazorDevicesComponentBase : ComponentBase, IDisposable
    {

        [Inject] public BlazorDevicesService BlazorDevicesService { get; set; }

        [Parameter] public string Id { get; set; }
        [Parameter] public BlazorDeviceItem AudioDefaultDevice { get; set; }
        [Parameter] public BlazorDeviceItem MicrophoneDefaultDevice { get; set; }
        [Parameter] public BlazorDeviceItem WebcamDefaultDevice { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.BlazorDevicesService.OnUpdateUI += UpdateUI;
            await base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.BlazorDevicesService.InitBlazorDevices();
                await this.BlazorDevicesService.InitBlazorDevicesMap();
                await this.BlazorDevicesService.GetDevices();

                var audioItem = this.BlazorDevicesService.item.audios.FirstOrDefault(item => item.id == this.AudioDefaultDevice.id);
                this.AudioDefaultDevice = audioItem ?? this.BlazorDevicesService.item.audios.FirstOrDefault();
                
                var microphoneItem = this.BlazorDevicesService.item.microphones.FirstOrDefault(item => item.id == this.MicrophoneDefaultDevice.id);
                this.MicrophoneDefaultDevice = microphoneItem ?? this.BlazorDevicesService.item.microphones.FirstOrDefault();
                
                var webcamItem = this.BlazorDevicesService.item.webcams.FirstOrDefault(item => item.id == this.WebcamDefaultDevice.id);
                this.WebcamDefaultDevice = webcamItem ?? this.BlazorDevicesService.item.webcams.FirstOrDefault();

                this.UpdateUI();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void SetAudioDefaultDevice(BlazorDeviceItem item)
        {
            this.AudioDefaultDevice = item;
            this.BlazorDevicesService.AudioDefaultDeviceChanged(this.Id, item);
            this.UpdateUI();
        }
        public void SetMicrophoneDefaultDevice(BlazorDeviceItem item)
        {
            this.MicrophoneDefaultDevice = item;
            this.BlazorDevicesService.MicrophoneDefaultDeviceChanged(this.Id, item);
            this.UpdateUI();
        }
        public void SetWebcamDefaultDevice(BlazorDeviceItem item)
        {
            this.WebcamDefaultDevice = item;
            this.BlazorDevicesService.WebcamDefaultDeviceChanged(this.Id, item);
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
            this.BlazorDevicesService.OnUpdateUI -= UpdateUI;
        }

    }
}
