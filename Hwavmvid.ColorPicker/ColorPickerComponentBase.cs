using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hwavmvid.ColorPicker
{
    public class ColorPickerComponentBase : ComponentBase
    {

        [Inject] public ColorPickerService ColorPickerService { get; set; }

        [Parameter] public string ContextColor { get; set; }
        [Parameter] public ColorPickerType ColorPickerType { get; set; }

        [Parameter]
        public Dictionary<string, dynamic> ColorSet { get; set; } = new Dictionary<string, dynamic>()
        {
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#8A2BE2" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#5F9EA0" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#FF7F50" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#6495ED" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#008B8B" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#DCDCDC" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#FF8C00" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#9932CC" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#1E90FF" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#FF69B4" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#F0E68C" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#20B2AA" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#66CDAA" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#C71585" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#FFDEAD" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#663399" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#BC8F8F" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#D2B48C" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#4682B4" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#D8BFD8" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#CD853F" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#CD5C5C" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#4B0082" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#A9A9A9" } },
        };

        protected override Task OnInitializedAsync()
        {
            if(ColorPickerType == ColorPickerType.CustomColorPicker && !string.IsNullOrEmpty(this.ColorSet.FirstOrDefault().Value?.itemcolor))
            {
                this.ContextColor = this.ColorSet.FirstOrDefault().Value.itemcolor;
            }

            return base.OnInitializedAsync();
        }

        public void ContextColor_OnChangeAsync(string color)
        {
            this.ContextColor = color;
            this.ColorPickerService.InvokeColorPickerEvent(color);
        }

        public void ColorSetItem_OnClicked(KeyValuePair<string, dynamic> clickedkvpair)
        {
            foreach(var checkedkvpair in this.ColorSet.Where(item => item.Value.itemchecked == true))
            {
                this.ColorSet[checkedkvpair.Key] = new { itemchecked = false, itemcolor = checkedkvpair.Value.itemcolor };
            }

            this.ColorSet[clickedkvpair.Key] = new { itemchecked = true, itemcolor = clickedkvpair.Value.itemcolor };
            this.ContextColor = clickedkvpair.Value.itemcolor;
            this.ColorPickerService.InvokeColorPickerEvent(clickedkvpair.Value.itemcolor);
            this.StateHasChanged();
        }

        public bool ShowCustomColorPicker { get; set; }
        public void ToggleCustomColorPicker()
        {
            this.ShowCustomColorPicker = !this.ShowCustomColorPicker;
        }

    }
}
