using System.Collections.Generic;

namespace BlazorDynamicLayout
{
    public class TabItem<TItemTab>
    {

        public TItemTab Item { get; set; }
        public int Index { get; set; }

    }
}
