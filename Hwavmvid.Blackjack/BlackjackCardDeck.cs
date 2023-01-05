using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hwavmvid.Blackjack
{
    public class BlackjackCardDeck
    {

        public List<BlackjackCard> Cards = new List<BlackjackCard>();
        public string ImageUrl { get; set; } = "carddeck";
        public string ImageUrlExtension { get; set; } = ".svg";

    }
}
