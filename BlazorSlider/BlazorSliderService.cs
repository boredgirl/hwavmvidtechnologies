using Microsoft.AspNetCore.Components;
using System;

namespace BlazorSlider
{
    public class BlazorSliderService
    {

        public event Action<BlazorSliderEvent> SliderValueOnChange;

        public void UpdateValue(object sender, ChangeEventArgs e, string id)
        {
            var item = new BlazorSliderEvent()
            {
                Id = id,
                SliderNewValue = Convert.ToInt32(e.Value),
            };

            this.SliderValueOnChange?.Invoke(item);
        }

    }
}
