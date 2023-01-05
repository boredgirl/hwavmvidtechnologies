using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hwavmvid.Rouletteshared.Items;
using Hwavmvid.Rouletteshared.Enums;
using Hwavmvid.Rouletteshared.Events;
using Hwavmvid.Rouletteshared.Constants;
using System.Linq;

namespace Hwavmvid.Roulettecoins
{

    public class RoulettecoinsService
    {

        private DotNetObjectReference<RoulettecoinsService> dotNetObjectReference { get; set; }

        public IJSRuntime jsruntime;
        public IJSObjectReference javascriptfile;

        public List<RouletteBetoptionsItem> betoptionitems { get; set; }
        public List<RoulettesurfaceNumber> surfacenumberitems { get; set; }

        public List<RoulettecoinsMap> draggablejsmaps { get; set; } = new List<RoulettecoinsMap>();
        public List<RoulettecoinsMap> droppablejsmaps { get; set; } = new List<RoulettecoinsMap>();

        public List<RouletteCoin> CoinItems { get; set; } = new List<RouletteCoin>();

        public event Action<RoulettecoinEvent> OnItemDropped;

        public RoulettecoinsService(IJSRuntime jsRuntime)
        {

            this.jsruntime = jsRuntime;
            this.dotNetObjectReference = DotNetObjectReference.Create(this);

            foreach (var item in Enum.GetValues<RoulettecoinsType>())
            {
                this.CoinItems.Add(new RouletteCoin(item));
            }
        }

        public async Task InitRouletteService()
        {
            if (this.javascriptfile == null)
                this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/roulettecoinsjsinterop.js");
        }
        public async Task InitJsMap(List<RouletteBetoptionsItem> betoptionitems, List<RoulettesurfaceNumber> surfacenumberitems)
        {

            this.betoptionitems = betoptionitems;
            this.surfacenumberitems = surfacenumberitems;

            await this.InitRouletteService();
            if (this.javascriptfile != null)
            {

                if (!this.draggablejsmaps.Any())
                {
                    foreach (var coinitem in this.CoinItems)
                    {
                        var obj = await this.javascriptfile.InvokeAsync<IJSObjectReference>("initroulettecoins", this.dotNetObjectReference, string.Concat(RouletteConstants.draggableitemprefix, coinitem.Id), "draggable");
                        
                        await obj.InvokeVoidAsync("removeevents");
                        await obj.InvokeVoidAsync("addevents");

                        this.draggablejsmaps.Add(new RoulettecoinsMap() { Id = coinitem.Id, JSObjectReference = obj });
                    }
                }                

                if (!this.droppablejsmaps.Any())
                {

                    foreach (var betoptionitem in betoptionitems)
                    {
                        var obj = await this.javascriptfile.InvokeAsync<IJSObjectReference>("initroulettecoins", this.dotNetObjectReference, string.Concat(RouletteConstants.droppableitemprefix, betoptionitem.Key.ToString()), "droppable");

                        await obj.InvokeVoidAsync("removeevents");
                        await obj.InvokeVoidAsync("addevents");

                        this.droppablejsmaps.Add(new RoulettecoinsMap() { Id = betoptionitem.Key.ToString(), JSObjectReference = obj });
                    }

                    foreach (var surfacenumberitem in surfacenumberitems)
                    {
                        var obj = await this.javascriptfile.InvokeAsync<IJSObjectReference>("initroulettecoins", this.dotNetObjectReference, string.Concat(RouletteConstants.droppableitemprefix, surfacenumberitem.Value.ToString()), "droppable");

                        await obj.InvokeVoidAsync("removeevents");
                        await obj.InvokeVoidAsync("addevents");

                        this.droppablejsmaps.Add(new RoulettecoinsMap() { Id = surfacenumberitem.Value.ToString(), JSObjectReference = obj });
                    }
                }
            }
        }
        
        
        [JSInvokable("ItemDropped")]
        public void ItemDropped(string coinid, string droppedfieldid)
        {
            RoulettecoinEvent e = new RoulettecoinEvent();

            var coinitem = this.CoinItems.FirstOrDefault(item => item.Id == coinid);
            e.CoinItem = coinitem;
            
            var surfaceitem = this.surfacenumberitems.FirstOrDefault(item => item.Value.ToString() == droppedfieldid);
            if (surfaceitem != null)
            {
                e.SurfaceNumber = surfaceitem;
            }

            var betoptionitem = this.betoptionitems.FirstOrDefault(item => item.Key.ToString() == droppedfieldid);
            e.BetoptionsItem = betoptionitem;

            if (e.CoinItem != null && (e.SurfaceNumber != null || e.BetoptionsItem != null))
            {
                this.OnItemDropped?.Invoke(e);
            }
        }
        public void Exposedroppeditem(RouletteCoin item)
        {
            this.OnItemDropped?.Invoke(new RoulettecoinEvent() { CoinItem = item });
        }

    }
}