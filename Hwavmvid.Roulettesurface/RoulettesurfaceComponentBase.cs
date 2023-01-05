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
        public RouletteNumber WinItem { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.RouletteService.OnWinItemDetected += WinItemDetected;
            await base.OnInitializedAsync();
        }

        public void WinItemDetected(RouletteEvent e)
        {
            this.WinItem = e.WinItem;
            this.StateHasChanged();
        }
        public void Dispose()
        {
            this.RouletteService.OnWinItemDetected -= WinItemDetected;
        }

    }
}
