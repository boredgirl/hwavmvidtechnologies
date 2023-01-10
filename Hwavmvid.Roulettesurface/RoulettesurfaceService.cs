using Hwavmvid.Rouletteshared.Items;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hwavmvid.Roulettesurface
{

    public class RoulettesurfaceService : IDisposable
    {

        private IJSRuntime jsruntime;
        private IJSObjectReference javascriptfile;

        public List<RoulettesurfaceNumber> NumberItems { get; set; } = new List<RoulettesurfaceNumber>();
        public RouletteNumber WinItem { get; set; }

        public event Action UpdateUI;

        public string Black { get; set; } = "black";
        public string Red { get; set; } = "red";
        public string Carpetgreen { get; set; } = "rgba(33,109,70,0.8)"; // #216d46
        public string Transparent { get; set; } = "transparent";

        public RoulettesurfaceService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
            this.NumberItems = this.GetSurfaceNumbers();
        }
        public async Task InitRoulettesurfaceService()
        {
            this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>(
               "import", "/Modules/Oqtane.ChatHubs/roulettesurfacejsinterop.js");
        }
        public async Task<string> Prompt(string message)
        {
            return await this.javascriptfile.InvokeAsync<string>("showPrompt", message);
        }
        public List<RoulettesurfaceNumber> GetSurfaceNumbers()
        {
            List<RoulettesurfaceNumber> items = new List<RoulettesurfaceNumber>();
            for (var i = 0; i <= 37; i++)
            {
                var item = new RoulettesurfaceNumber()
                {
                    Value = i,
                    Color = i == 0 ? this.Carpetgreen : i % 2 == 0 ? this.Black : this.Red,
                };

                items.Add(item);
            }

            return items;
        }

        public void InvokeUpdateUI()
        {
            this.UpdateUI?.Invoke();
        }

        public void Dispose()
        {
            if (javascriptfile != null)
                this.javascriptfile.DisposeAsync();
        }

    }
}