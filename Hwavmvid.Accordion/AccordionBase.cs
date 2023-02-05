using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Hwavmvid.Accordion
{
    public class AccordionBase : ComponentBase
    {

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Id { get; set; }

        public IList<AccordionItem> AccordionItems { get; set; } = new List<AccordionItem>();

        public AccordionItem ActiveAccordionItem { get; set; }

        public void AddAccordionItem(AccordionItem item)
        {
            if(!this.AccordionItems.Contains(item))
            {
                this.AccordionItems.Add(item);
            }
        }

    }
}
