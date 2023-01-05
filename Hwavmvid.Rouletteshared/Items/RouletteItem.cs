using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hwavmvid.Rouletteshared.Items
{

    public class RouletteItem
    {

        public int RowId { get; set; }
        public int ColumnId { get; set; }
        public string Id { get; set; }
        public int ZIndex { get; set; }
        public double Opacity { get; set; }
        public string BackgroundColor { get; set; }
        public int Rotation { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlExtension { get; set; }
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public int Value { get; set; }

    }
    public class RouletteBall : RouletteItem {}
    public class RouletteNumber : RouletteItem {}
    public class RouletteNumbers : RouletteItem {}
    public class RouletteBallRaceway : RouletteItem {}
    public class RouletteCarpet : RouletteItem {}

}
