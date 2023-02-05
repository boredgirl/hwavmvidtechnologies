using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorNotifications
{
    public class NotificationsService
    {

        private IJSRuntime jsRuntime { get; set; }
        private IJSObjectReference moduleTask { get; set; }
        private IJSObjectReference notificationsMap { get; set; }
        public DotNetObjectReference<NotificationsService> dotNetObjectReference { get; set; }

        public Action OnUpdateUI { get; set; }
        public EventHandler<NotificationItem> OnNotificationAdded { get; set; }

        public List<NotificationItem> NotificationItems = new List<NotificationItem>();

        public NotificationsService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
            this.dotNetObjectReference = DotNetObjectReference.Create(this);
        }
        public async Task InitNotificationsServiceAsync()
        {
            this.moduleTask = await this.jsRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/blazornotificationsjsinterop.js");
            this.notificationsMap = await this.moduleTask.InvokeAsync<IJSObjectReference>("initnotifications", this.dotNetObjectReference);
        }

        public void AddNotification(NotificationItem item)
        {
            this.NotificationItems.Add(item);
            this.OnNotificationAdded?.Invoke(this, item);

            this.OnUpdateUI?.Invoke();
        }

        public void RemoveNotification(string id)
        {
            var item = this.NotificationItems.FirstOrDefault(item => item.Id == id);
            if(item != null)
            {
                this.NotificationItems.Remove(item);
                this.OnUpdateUI?.Invoke();
            }
        }

    }
}
