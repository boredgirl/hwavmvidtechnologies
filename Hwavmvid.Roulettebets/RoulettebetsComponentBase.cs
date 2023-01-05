using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Hwavmvid.Rouletteshared.Items;

namespace Hwavmvid.Roulettebets
{
    public class RoulettebetsComponentBase : ComponentBase, IDisposable
    {

        [Inject] public RouletteBetsService RouletteBetsService { get; set; }
        [Parameter] public RouletteBetItem Item { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.RouletteBetsService.OnUpdateComponent += UpdateUI;
            await base.OnInitializedAsync();
        }
        public void Remove_Clicked(RouletteBetItem item)
        {
            this.RouletteBetsService.RemoveBetItem(item.Id);
        }
        public void UpdateUI()
        {
            this.InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }
        public void Dispose()
        {
            this.RouletteBetsService.OnUpdateComponent -= UpdateUI;
        }

    }
}
