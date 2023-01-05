using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorNotifications
{
    public class BlazorNotificationsBase : ComponentBase, IDisposable
    {

        [Inject] protected BlazorNotificationsService BlazorNotificationsService { get; set; }

        protected override void OnInitialized()
        {
            this.BlazorNotificationsService.OnUpdateUI += this.OnUpdateUIExecute;
            this.BlazorNotificationsService.OnNotificationAdded += (object sender, NotificationItem item) => this.OnNotificationAddedExecuteAsync(sender, item);
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.BlazorNotificationsService.InitNotificationsServiceAsync();
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
            this.BlazorNotificationsService.RemoveNotification(item.Id);
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        private async void OnUpdateUIExecute()
        {
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        public void Dispose()
        {
            this.BlazorNotificationsService.OnUpdateUI -= this.OnUpdateUIExecute;
            this.BlazorNotificationsService.OnNotificationAdded -= (object sender, NotificationItem item) => this.OnNotificationAddedExecuteAsync(sender, item);
        }

    }
}
