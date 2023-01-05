using Hwavmvid.Rouletteshared.Enums;
using System;

namespace Hwavmvid.Rouletteshared.Items
{
    public class RouletteCoin
    {

        public RouletteCoin(RoulettecoinsType type)
        {

            this.Id = Guid.NewGuid().ToString().Replace("-", "_");

            if (type == RoulettecoinsType.OneHundred)
                this.Value = 100;

            if (type == RoulettecoinsType.TwoHundred)
                this.Value = 200;

            if (type == RoulettecoinsType.FiveHundred)
                this.Value = 500;

            if (type == RoulettecoinsType.OneThousend)
                this.Value = 1000;

            if (type == RoulettecoinsType.FourThousend)
                this.Value = 4000;

            if (type == RoulettecoinsType.TenThousend)
                this.Value = 10000;

            if (type == RoulettecoinsType.TwentyThousend)
                this.Value = 20000;

        }

        public string Id { get; set; }
        public int Value { get; set; }
        public string Currency { get; set; } = "Dollar";
        public string CurrencyAbbr { get; set; } = "$";
        public RoulettecoinsType Type { get; set; }

    }
}
