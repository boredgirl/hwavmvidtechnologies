using System.Collections.Generic;
using Hwavmvid.Rouletteshared.Items;

namespace Hwavmvid.Roulette
{
    public class RouletteMap
    {
        public List<RouletteRow> Rows = new List<RouletteRow>();
        public List<RouletteColumn> Columns = new List<RouletteColumn>();
    }
    public class RouletteRow
    {
        public int RowId { get; set; }
    }
    public class RouletteColumn
    {
        // xy coordinates
        public int ColumnId { get; set; }
        public int RowId { get; set; }

        // column object items
        public List<RouletteItem> Items { get; set; } = new List<RouletteItem>();        
    }
    public enum RouletteDirection
    {
        left,
        right,
    }    

}
