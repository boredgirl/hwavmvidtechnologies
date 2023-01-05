using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hwavmvid.Motorsport.Shared.Items;
using System.Linq;

namespace Hwavmvid.Motorsport.Racewaymaps
{

    public class Motorsportracewayservice : IDisposable
    {

        private IJSRuntime jsruntime;
        private IJSObjectReference javascriptfile;
        private DotNetObjectReference<Motorsportracewayservice> serviceobjref;

        public MRacewaymap Map { get; set; }
        public List<DraggableObject> DraggableItems { get; set; } = new List<DraggableObject>();

        public event Action UpdateUI;
        public event Action OnUpdateComponent;
        public event Action<MRacewayevent> Onitemremoved;
        public event Action<MRacewaymapitem<MRacewayitemtype>> ItemRemoved;

        public Motorsportracewayservice(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
            this.serviceobjref = DotNetObjectReference.Create(this);
        }
        public async Task InitMotorsportracewayService()
        {

            this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/hwavmvidmotorsportjsinterop.js");
            if (this.javascriptfile != null)
            {
                foreach (var draggableitem in this.DraggableItems)
                {
                    var obj = await this.javascriptfile.InvokeAsync<IJSObjectReference>("initmotorsportmap", this.serviceobjref, Shared.MotorsportConstants.draggableitemprefix + draggableitem.ElementId, "draggable");
                    await obj.InvokeVoidAsync("removeevents");
                    await obj.InvokeVoidAsync("addevents");
                }

                foreach (var column in this.Map.Columns)
                {
                    var obj = await this.javascriptfile.InvokeAsync<IJSObjectReference>("initmotorsportmap", this.serviceobjref, Shared.MotorsportConstants.droppableitemprefix + column.ElementId, "droppable");
                    await obj.InvokeVoidAsync("removeevents");
                    await obj.InvokeVoidAsync("addevents");
                }
            }
        }

        public MRacewaycolumn GetMapColumn(int rowid, int colid)
        {
            return this.Map.Columns.FirstOrDefault(item => item.RowId == rowid && item.ColumnId == colid);
        }
        public void AddMapColumnItem(int rowid, int colid, MRacewaymapitem<MRacewayitemtype> item)
        {

            try
            {
                var col = this.GetMapColumn(rowid, colid);
                if (col != null)
                {
                    var itemlist = col.GetColumnItemsGenericlistBytype(item.Racewayitemtype);
                    if (itemlist != null)
                    {
                        itemlist.Add(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }
        
        public void Createdraggableitems()
        {
            foreach (var itemtype in Enum.GetValues(typeof(MRacewayitemtype)))
                this.DraggableItems.Add(new DraggableObject() { Type = (MRacewayitemtype)itemtype });
        }

        public void HandleItemRemoved(MRacewaymapitem<MRacewayitemtype> item)
        {
            MRacewayevent e = new MRacewayevent() { Item = item };
            this.Onitemremoved?.Invoke(e);
        }
        public void UpdateComponent()
        {
            this.OnUpdateComponent?.Invoke();
        }
        public void Dispose()
        {
            if (javascriptfile != null)
                this.javascriptfile.DisposeAsync();
        }

    }
}
