using System.Collections.Generic;

namespace BlazorPager
{
    public class BlazorPagerApiItem<TItemGeneric>
    {

        public List<TItemGeneric> Items { get; set; }
        public int Pages { get; set; }

    }
}
