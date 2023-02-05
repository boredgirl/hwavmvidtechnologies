using BlazorSelect;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hwavmvid.Video
{
    public class VideoComponentBase : ComponentBase, IDisposable
    {

        [Inject] public VideoService VideoService { get; set; }
        [Parameter] public string Id1 { get; set; }
        [Parameter] public string Id2 { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string BackgroundColor { get; set; }
        [Parameter] public VideoType Type { get; set; }
        [Parameter] public VideoStatusType Status { get; set; }
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

        public VideoSourceType VideoSourceSelectedItem { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.InitVideoSourceSelection();
            this.VideoService.RunUpdateUI += UpdateUIStateHasChanged;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.InitDevices();

                await this.VideoService.InitVideo();
                await this.VideoService.InitVideoMap(this.Id1, this.Id2, this.Type, this.VideoSourceSelectedItem, this.Framerate, this.VideoBitsPerSecond, this.AudioBitsPerSecond, this.VideoSegmentsLength, this.AudioDefaultDeviceId, this.MicrophoneDefaultDeviceId, this.WebcamDefaultDeviceId);

                try
                {
                    await this.VideoService.ContinueLocalLivestreamAsync(this.Id1, this.Id2);
                }
                catch (Exception exception)
                {
                    this.VideoService.ThrowError(exception.Message);
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

            var newVideoSourceSelectedItem = (VideoSourceType)Enum.Parse(typeof(VideoSourceType), e.SelectedItem);

            var map = this.VideoService.GetVideoMap(this.Id1, this.Id2);
            if (map != null)
            {
                await this.VideoService.StopVideoChat(map.Id1, map.Id2);
                this.VideoService.RemoveVideoMap(map.MapId);
                await this.VideoService.InitVideoMap(this.Id1, this.Id2, this.Type, newVideoSourceSelectedItem, this.Framerate, this.VideoBitsPerSecond, this.AudioBitsPerSecond, this.VideoSegmentsLength, this.AudioDefaultDeviceId, this.MicrophoneDefaultDeviceId, this.WebcamDefaultDeviceId);
            }

            this.VideoSourceSelectedItem = newVideoSourceSelectedItem;

            this.StateHasChanged();
        }

        private void InitVideoSourceSelection()
        {
            foreach (VideoSourceType source in (VideoSourceType[])Enum.GetValues(typeof(VideoSourceType)))
            {
                this.VideoSourceSelectionItems.Add(source.ToString());
            }
        }

        private async void UpdateUIStateHasChanged(string id1, string id2)
        {
            var map = this.VideoService.GetVideoMap(id1, id2);
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
            var map = VideoService.VideoMaps.Where(item => item.Id1 == Id1 && item.Id2 == Id2).FirstOrDefault();
            if(map != null)
            {
                map.VideoOverlay = true;
            }

            this.VideoService.RunUpdateUI -= UpdateUIStateHasChanged;
        }

    }
}
