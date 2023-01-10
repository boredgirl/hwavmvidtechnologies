using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Enums;

namespace Hwavmvid.Roulette
{

    public class RouletteService
    {

        private IJSRuntime jsruntime;

        public event Action OnPlayNewRouletteGame;
        public event Action OnStopRouletteGame;
        public event Action<RouletteEvent> OnWinItemDetected;

        public RouletteGameStatus GameStatus { get; set; }

        public RouletteService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
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

    }
}