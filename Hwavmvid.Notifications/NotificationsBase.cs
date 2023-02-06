using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Hwavmvid.Notifications
{
    public class NotificationsBase : ComponentBase, IDisposable
    {

        [Inject] protected NotificationsService NotificationsService { get; set; }

        protected override void OnInitialized()
        {
            this.NotificationsService.OnUpdateUI += this.OnUpdateUIExecute;
            this.NotificationsService.OnNotificationAdded += (object sender, NotificationItem item) => this.OnNotificationAddedExecuteAsync(sender, item);
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.NotificationsService.InitNotificationsServiceAsync();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private void OnNotificationAddedExecuteAsync(object sender, NotificationItem e)
        {
            this.DelayedTask(e);
        }

        private async void DelayedTask(NotificationItem item)
        {
            await Task.Delay(item.Timeout);
            this.NotificationsService.RemoveNotification(item.Id);
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        private async void OnUpdateUIExecute()
        {
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        public void Dispose()
        {
            this.NotificationsService.OnUpdateUI -= this.OnUpdateUIExecute;
            this.NotificationsService.OnNotificationAdded -= (object sender, NotificationItem item) => this.OnNotificationAddedExecuteAsync(sender, item);
        }

    }
}
