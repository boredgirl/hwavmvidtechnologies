using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hwavmvid.Blackjack
{
    public class BlackjackCard
    {

        public string Id { get; set; }
        public byte Value { get; set; }
        public string Abbr { get; set; }
        public BlackjackCardType Type { get; set; }
        public BlackjackCardColor Color { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlExtension { get; set; } = ".svg";
        public string ImageFontColor { get; set; }

    }
}
