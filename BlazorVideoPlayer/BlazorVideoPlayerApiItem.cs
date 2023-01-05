namespace BlazorVideoPlayer
{
    public class BlazorVideoPlayerApiItem
    {

        public string Base64DataUri { get; set; }
        public string LastSequence { get; set; }
        public int TotalSequences { get; set; }
        public bool SliderValueChanged { get; set; }
        public int SliderCurrentValue { get; set; }

    }
}
