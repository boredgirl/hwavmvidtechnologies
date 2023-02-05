using System.Collections.Generic;

namespace Hwavmvid.Devices
{
    public class DevicesItem
    {
        public List<DeviceItem> audios { get; set; }
        public List<DeviceItem> microphones { get; set; }
        public List<DeviceItem> webcams { get; set; }
    }

    public class DeviceItem
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
