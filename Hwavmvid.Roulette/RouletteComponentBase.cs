using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Enums;

namespace Hwavmvid.Roulette
{

    public class RouletteComponentBase : ComponentBase, IAsyncDisposable
    {

        [Inject] public RouletteService RouletteService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.RouletteService.UupdateUI += async () => await this.UpdateUI();
            await base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                // if not already initialized roulette service ??
                if (string.IsNullOrEmpty(RouletteService.carpet.BackgroundColor))
                {
                    this.RouletteService.Map = this.RouletteService.GetRouletteMap();
                }

                if (RouletteService.GameStatus != RouletteGameStatus.StartNewGame)
                {
                    this.RouletteService.RemoveRouletteItem(this.RouletteService.ball.RowId, this.RouletteService.ball.ColumnId, this.RouletteService.ball);
                    this.RouletteService.ResetVariables();
                }

                // if not already initialized roulette service ??
                if (string.IsNullOrEmpty(RouletteService.carpet.BackgroundColor))
                {
                    this.RouletteService.NumberItems = this.RouletteService.GetRouletteNumbers();

                    this.RouletteService.InitRouletteCarpet();
                    this.RouletteService.InitRouletteNumbers();
                    this.RouletteService.InitRouletteRaceway();

                    this.RouletteService.RunRouletteNumbers();
                }

                this.RouletteService.GameStatus = RouletteGameStatus.StartNewGame;
                this.RouletteService.loading = false;
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public async Task UpdateUI()
        {
            await this.InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }

        public async ValueTask DisposeAsync()
        {
            this.RouletteService.UupdateUI += async () => await this.UpdateUI();
        }

    }
}
