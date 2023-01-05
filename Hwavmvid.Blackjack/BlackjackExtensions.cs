using System;
using System.Collections.Generic;
using System.Linq;

namespace Hwavmvid.Blackjack
{
    public static class BlackjackExtensions
    {
        public static void Shuffle<BlackjackCard>(this IList<BlackjackCard> items)
        {
            int i = items.Count();
            Random rndm = new Random();
            while (i > 1)
            {
                i--;
                int next = rndm.Next(i + 1);
                BlackjackCard item = items[next];
                items[next] = items[i];
                items[i] = item;
            }
        }
    }
}
