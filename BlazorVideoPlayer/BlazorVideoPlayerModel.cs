using Microsoft.JSInterop;
using System;

namespace BlazorVideoPlayer
{
    public class BlazorVideoPlayerModel
    {

        public string MapId { get; set; }
        public string ParameterId1 { get; set; }
        public string ParameterId2 { get; set; }
        public string LastSequenceId { get; set; }
        public int SliderCurrentValue { get; set; }
        public bool SliderValueChanged { get; set; }
        public int SliderTotalSequences { get; set; }
        public IJSObjectReference JsObjRef { get; set; }
        public bool VideoOverlay { get; set; }

    }
}
