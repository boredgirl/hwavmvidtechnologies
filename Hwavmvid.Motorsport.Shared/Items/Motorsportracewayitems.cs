using System;
using System.Collections.Generic;
using System.Linq;

namespace Hwavmvid.Motorsport.Shared.Items
{

    public class MRacewaymap
    {

        public List<MRacewayrow> Rows { get; set; } = new List<MRacewayrow>();
        public List<MRacewaycolumn> Columns { get; set; } = new List<MRacewaycolumn>();

    }
    public class MRacewayrow
    {

        public int RowId { get; set; }

    }
    public class MRacewaycolumn
    {

        public int ColumnId { get; set; }
        public int RowId { get; set; }

        public string ElementId { get; set; } = Guid.NewGuid().ToString().Replace("-", "_");

        public List<MRacewaymapitem<MRacewayitemtype>> Racecars { get; set; } = new List<MRacewaymapitem<MRacewayitemtype>>();
        public List<MRacewaymapitem<MRacewayitemtype>> Streetways { get; set; } = new List<MRacewaymapitem<MRacewayitemtype>>();
        public List<MRacewaymapitem<MRacewayitemtype>> Buildings { get; set; } = new List<MRacewaymapitem<MRacewayitemtype>>();
        public List<MRacewaymapitem<MRacewayitemtype>> Platforms { get; set; } = new List<MRacewaymapitem<MRacewayitemtype>>();

        public List<MRacewaymapitem<MRacewayitemtype>> GetColumnItemsGenericlistBytype(MRacewayitemtype itemtype) {

            return itemtype == MRacewayitemtype.Racecar ? this.Racecars :
                   itemtype == MRacewayitemtype.Streetway ? this.Streetways :
                   itemtype == MRacewayitemtype.Building ? this.Buildings :
                   itemtype == MRacewayitemtype.Platform ? this.Platforms : 
                   null;
        }
    
    }
    public class MRacewaymapitem<ItemType>
    {

        public MRacewaymapitem(string id, MRacewayitemtype itemtype)
        {
            this.Id = id;
            this.Racewayitemtype = itemtype;
        }

        public string Id { get; set; }
        public MRacewayitemtype Racewayitemtype { get; set; }

        public int RowId { get; set; }
        public int ColumnId { get; set; }
        public int ZIndex { get; set; }        
        public string BackgroundColor { get; set; }
        public int Rotation { get; set; }
        public int Value { get; set; }
        public double Opacity { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlExtension { get; set; }
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }

    }
    public class MRacewayevent
    {

        public MRacewaymapitem<MRacewayitemtype> Item { get; set; }

    }
    public enum MRacewayitemtype
    {

        Racecar,
        Streetway,
        Building,
        Platform,

    }
    public class DraggableObject
    {

        public string ElementId { get; set; } = Guid.NewGuid().ToString().Replace("-", "_");
        public MRacewayitemtype Type { get; set; }

    }

}
