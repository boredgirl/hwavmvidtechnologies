using System.Collections.Generic;

namespace BlazorDevices
{
    public class BlazorDevicesItem
    {
        public List<BlazorDeviceItem> audios { get; set; }
        public List<BlazorDeviceItem> microphones { get; set; }
        public List<BlazorDeviceItem> webcams { get; set; }
    }

    public class BlazorDeviceItem
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
