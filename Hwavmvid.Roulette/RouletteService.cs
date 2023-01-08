using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Enums;

namespace Hwavmvid.Roulette
{

    public class RouletteService : IDisposable
    {

        private IJSObjectReference javascriptfile;
        private IJSRuntime jsruntime;

        public event Action OnPlayNewRouletteGame;
        public event Action OnStopRouletteGame;
        public event Action<RouletteEvent> OnWinItemDetected;

        public RouletteGameStatus GameStatus = RouletteGameStatus.StartNewGame;

        public RouletteService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
        }
        public async Task InitRouletteService()
        {
            this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>(
               "import", "/Modules/Oqtane.ChatHubs/roulettejsinterop.js");
        }
        public void PlayNewRouletteGame()
        {
            this.OnPlayNewRouletteGame?.Invoke();
        }
        public void StopRouletteGame()
        {
            this.OnStopRouletteGame?.Invoke();
        }
        public void ExposeWinItem(RouletteNumber item)
        {
            this.OnWinItemDetected?.Invoke(new RouletteEvent() { WinItem = item });
        }
        public async Task<string> Prompt(string message)
        {
            return await this.javascriptfile.InvokeAsync<string>("showPrompt", message);
        }
        public void Dispose()
        {
            if (javascriptfile != null)
                this.javascriptfile.DisposeAsync();
        }

    }
}