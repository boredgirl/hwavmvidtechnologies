using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Items;
using System.Collections.Generic;
using System.Linq;

namespace Hwavmvid.Roulettebets
{

    public class RouletteBetsService : IDisposable
    {

        private IJSObjectReference javascriptfile;
        private IJSRuntime jsruntime;

        public List<RouletteBetItem> BetItems { get; set; } = new List<RouletteBetItem>();

        public event Action<RoulettebetsEvent> OnBetRemoved;
        public event Action UpdateUI;
        public event Action OnUpdateComponent;
        public event Action<RouletteBetItem> ItemRemoved;

        public RouletteBetsService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
        }
        public async Task InitRouletteBetsService()
        {
            this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>(
               "import", "/Modules/Oqtane.ChatHubs/roulettebetsjsinterop.js");
        }
        public void AddBetItem(RouletteBetItem betitem)
        {
            if (this.BetItems.Find(item => item.Id.Equals(betitem.Id)) == null)
            {
                this.BetItems.Add(betitem);
                this.UpdateUI?.Invoke();
            }            
        }
        public void RemoveBetItem(string id)
        {
            var item = this.BetItems.FirstOrDefault(item => item.Id == id);
            if (item != null)
            {
                this.BetItems.Remove(item);
                this.ItemRemoved?.Invoke(item);
            }
        }
        public void ExposeRemovedItem(RouletteBetItem item)
        {
            RoulettebetsEvent e = new RoulettebetsEvent() { Item = item };
            this.OnBetRemoved?.Invoke(e);
        }
        public void UpdateComponent()
        {
            this.OnUpdateComponent?.Invoke();
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