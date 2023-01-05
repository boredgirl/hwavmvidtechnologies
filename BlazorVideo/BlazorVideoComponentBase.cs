using BlazorSelect;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase, IDisposable
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }
        [Parameter] public string Id1 { get; set; }
        [Parameter] public string Id2 { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string BackgroundColor { get; set; }
        [Parameter] public BlazorVideoType Type { get; set; }
        [Parameter] public BlazorVideoStatusType Status { get; set; }
        [Parameter] public int Viewers { get; set; }
        [Parameter] public int Framerate { get; set; }
        [Parameter] public int VideoBitsPerSecond { get; set; }
        [Parameter] public int AudioBitsPerSecond { get; set; }
        [Parameter] public int VideoSegmentsLength { get; set; }
        [Parameter] public string AudioDefaultDeviceId { get; set; }
        [Parameter] public string MicrophoneDefaultDeviceId { get; set; }
        [Parameter] public string WebcamDefaultDeviceId { get; set; }

        public bool IsVideoOverlay { get; set; } = true;

        public HashSet<string> VideoSourceSelectionItems = new HashSet<string>();

        public BlazorVideoSourceType VideoSourceSelectedItem { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.InitVideoSourceSelection();
            this.BlazorVideoService.RunUpdateUI += UpdateUIStateHasChanged;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.InitDevices();

                await this.BlazorVideoService.InitBlazorVideo();
                await this.BlazorVideoService.InitBlazorVideoMap(this.Id1, this.Id2, this.Type, this.VideoSourceSelectedItem, this.Framerate, this.VideoBitsPerSecond, this.AudioBitsPerSecond, this.VideoSegmentsLength, this.AudioDefaultDeviceId, this.MicrophoneDefaultDeviceId, this.WebcamDefaultDeviceId);

                try
                {
                    await this.BlazorVideoService.ContinueLocalLivestreamAsync(this.Id1, this.Id2);
                }
                catch (Exception exception)
                {
                    this.BlazorVideoService.ThrowError(exception.Message);
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void InitDevices()
        {
            if (string.IsNullOrEmpty(this.AudioDefaultDeviceId))
                this.AudioDefaultDeviceId = null;

            if (string.IsNullOrEmpty(this.MicrophoneDefaultDeviceId))
                this.MicrophoneDefaultDeviceId = null;

            if (string.IsNullOrEmpty(this.WebcamDefaultDeviceId))
                this.WebcamDefaultDeviceId = null;
        }

        public async void OnVideoOptionSelected(BlazorSelectEvent e)
        {
            this.InitDevices();

            var newVideoSourceSelectedItem = (BlazorVideoSourceType)Enum.Parse(typeof(BlazorVideoSourceType), e.SelectedItem);

            var map = this.BlazorVideoService.GetBlazorVideoMap(this.Id1, this.Id2);
            if (map != null)
            {
                await this.BlazorVideoService.StopVideoChat(map.Id1, map.Id2);
                this.BlazorVideoService.RemoveBlazorVideoMap(map.MapId);
                await this.BlazorVideoService.InitBlazorVideoMap(this.Id1, this.Id2, this.Type, newVideoSourceSelectedItem, this.Framerate, this.VideoBitsPerSecond, this.AudioBitsPerSecond, this.VideoSegmentsLength, this.AudioDefaultDeviceId, this.MicrophoneDefaultDeviceId, this.WebcamDefaultDeviceId);
            }

            this.VideoSourceSelectedItem = newVideoSourceSelectedItem;

            this.StateHasChanged();
        }

        private void InitVideoSourceSelection()
        {
            foreach (BlazorVideoSourceType source in (BlazorVideoSourceType[])Enum.GetValues(typeof(BlazorVideoSourceType)))
            {
                this.VideoSourceSelectionItems.Add(source.ToString());
            }
        }

        private async void UpdateUIStateHasChanged(string id1, string id2)
        {
            var map = this.BlazorVideoService.GetBlazorVideoMap(id1, id2);
            if (this.Id1 == map.Id1 && this.Id2 == map.Id2)
            {
                await InvokeAsync(() =>
                {
                    this.StateHasChanged();
                });
            }
        }

        public void Dispose()
        {
            var map = BlazorVideoService.BlazorVideoMaps.Where(item => item.Id1 == Id1 && item.Id2 == Id2).FirstOrDefault();
            if(map != null)
            {
                map.VideoOverlay = true;
            }

            this.BlazorVideoService.RunUpdateUI -= UpdateUIStateHasChanged;
        }

    }
}
