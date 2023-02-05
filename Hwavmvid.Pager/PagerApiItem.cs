using System.Collections.Generic;

namespace Hwavmvid.Pager
{
    public class PagerApiItem<TItemGeneric>
    {

        public List<TItemGeneric> Items { get; set; }
        public int Pages { get; set; }

    }
}
