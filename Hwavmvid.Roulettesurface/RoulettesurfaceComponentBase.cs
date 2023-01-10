using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Hwavmvid.Roulette;
using System.Collections.Generic;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Events;

namespace Hwavmvid.Roulettesurface
{
    public class RoulettesurfaceComponentBase : ComponentBase, IDisposable
    {

        [Inject] public RoulettesurfaceService RoulettesurfaceService { get; set; }
        [Inject] public RouletteService RouletteService { get; set; }

        public const int NumberItemsContainerHeight = 400;

        protected override async Task OnInitializedAsync()
        {
            this.RoulettesurfaceService.UpdateUI += UpdateUI;
            await base.OnInitializedAsync();
        }

        public void UpdateUI()
        {
            this.StateHasChanged();
        }

        public void Dispose()
        {
            this.RoulettesurfaceService.UpdateUI -= UpdateUI;
            this.StateHasChanged();
        }
    }
}
