using System;

namespace BlazorDynamicLayout
{
    public class BlazorDynamicLayoutService
    {

        public event Action<TabItemEvent> TabItemClickedEvent;
        public event Action<string> OnErrorEvent;

        public void TabClicked(string id)
        {
            try
            {
                TabItemEvent tabEvent = new TabItemEvent();
                tabEvent.ActivatedItemId = id;
                this.TabItemClickedEvent.Invoke(tabEvent);
            }
            catch (Exception exception)
            {
                this.ThrowError(exception.Message);
            }
        }

        public void ThrowError(string message)
        {
            this.OnErrorEvent?.Invoke(message);
        }

    }
}
