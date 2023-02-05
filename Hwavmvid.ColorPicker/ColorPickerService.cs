using System;

namespace Hwavmvid.ColorPicker
{
    public class ColorPickerService
    {

        public event Action<ColorPickerEvent> OnColorPickerContextColorChangedEvent;

        public void InvokeColorPickerEvent(string color)
        {
            this.OnColorPickerContextColorChangedEvent?.Invoke(
                new ColorPickerEvent() { ContextColor = color });
        }

    }
}
