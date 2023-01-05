using BlazorSlider;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorVideoPlayer
{
    public class BlazorVideoPlayerComponentBase : ComponentBase, IDisposable
    {

        [Inject] public BlazorVideoPlayerService BlazorVideoPlayerService { get; set; }
        [Inject] public BlazorSliderService BlazorSliderService { get; set; }
        [Parameter] public string MapId { get; set; }
        [Parameter] public string ParameterId1 { get; set; }
        [Parameter] public string ParameterId2 { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string BackgroundColor { get; set; }
        [Parameter] public int TotalVideoSequences { get; set; }

        public bool VideoOverlay { get; set; } = true;
        public int SliderCurrentValue { get; set; } = 1;
        public bool SliderValueChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.BlazorSliderService.SliderValueOnChange += async (obj) => await OnSliderValueChangeExecute(obj);
            this.BlazorVideoPlayerService.RunUpdateUI += UpdateUIStateHasChanged;

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) 
            {
                await this.BlazorVideoPlayerService.InitBlazorVideoPlayer();
                await this.BlazorVideoPlayerService.InitBlazorVideoMap(this.MapId, this.ParameterId1, this.ParameterId2);
                await this.BlazorVideoPlayerService.GetFirstSequence(this.MapId);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnSliderValueChangeExecute(BlazorSliderEvent obj)
        {
            var map = this.BlazorVideoPlayerService.GetBlazorVideoMap(obj.Id);
            if (map != null)
            {
                if (!map.SliderValueChanged)
                {
                    map.LastSequenceId = string.Empty;
                    map.SliderCurrentValue = obj.SliderNewValue;
                    map.SliderValueChanged = true;

                    this.BlazorVideoPlayerService.BlazorVideoServiceExtension.GetNextSequenceOnSliderValueChanged(map.MapId);
                    await this.BlazorVideoPlayerService.ClearVideoBuffer(map.MapId);
                }
            }
        }

        private void UpdateUIStateHasChanged(string mapId)
        {
            if (this.MapId == MapId)
            {
                var map = this.BlazorVideoPlayerService.GetBlazorVideoMap(this.MapId);
                if (map != null)
                {
                    this.InvokeAsync(() =>
                    {
                        this.VideoOverlay = map.VideoOverlay;
                        this.SliderCurrentValue = map.SliderCurrentValue;
                        this.SliderValueChanged = map.SliderValueChanged;
                        this.TotalVideoSequences = map.SliderTotalSequences;

                        this.StateHasChanged();
                    });
                }
            }
        }

        public void Dispose()
        {
            this.BlazorSliderService.SliderValueOnChange -= async (obj) => await OnSliderValueChangeExecute(obj);
            this.BlazorVideoPlayerService.RunUpdateUI -= UpdateUIStateHasChanged;
        }

    }
}
