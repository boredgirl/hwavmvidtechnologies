using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Reflection;

namespace BlazorDynamicLayout
{
    public class BlazorDynamicLayoutBase<TItemLivestream, TItemTab, TItemContent> : ComponentBase
    {

        [Inject] public BlazorDynamicLayoutService BlazorDynamicLayoutService { get; set; }

        [Parameter] public RenderFragment<TItemLivestream> LivestreamItemContainer { get; set; }
        [Parameter] public RenderFragment<TabItem<TItemTab>> TabItemContainer { get; set; }
        [Parameter] public RenderFragment<ContentItem<TItemContent>> ContentItemContainer { get; set; }

        [Parameter] public List<TItemLivestream> Livestreams { get; set; }
        [Parameter] public List<TItemTab> Tabs { get; set; }
        [Parameter] public List<TItemContent> Contents { get; set; }

        [Parameter] public string TabIdPropertyInfoName { get; set; }
        
        [Parameter] public string DraggableLivestreamContainerElementId { get; set; }
        [Parameter] public string TabNavigationId { get; set; }
        [Parameter] public string TabContentId { get; set; }
        [Parameter] public string ActiveTabId { get; set; }

    }
}
