using System.Collections.Generic;

namespace BlazorDynamicLayout
{
    public class ContentItem<TItemContent>
    {

        public TItemContent Item { get; set; }
        public int Index { get; set; }

    }
}
