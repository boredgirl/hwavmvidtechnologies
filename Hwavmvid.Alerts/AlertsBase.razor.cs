using Microsoft.AspNetCore.Components;
using System;

namespace Hwavmvid.Alerts
{
    public class AlertsBase : ComponentBase, IDisposable
    {

        [Inject] public AlertsService AlertsService { get; set; }

        protected override void OnInitialized()
        {
            this.AlertsService.OnAlert += OnAlertExecute;
            base.OnInitialized();
        }

        public async void OnAlertExecute(string message, string heading, PositionType position, bool confirmDialog, string id)
        {
            await this.InvokeAsync(() =>
            {
                AlertsModel alert = new AlertsModel()
                {
                    Id = !string.IsNullOrEmpty(id) ? id : Guid.NewGuid().ToString(),
                    Message = message,
                    Headline = heading,
                    Position = position,
                    ConfirmDialog = confirmDialog,
                    CreatedOn = DateTime.Now
                };

                this.AlertsService.AddAlert(alert);
                this.StateHasChanged();
            });
        }

        public void CloseAlert_OnClicked(string id)
        {
            this.AlertsService.RemoveAlert(id);
        }

        public void ConfirmAlert_OnClicked(AlertsModel model)
        {
            this.AlertsService.RemoveAlert(model.Id);
            if(model.ConfirmDialog)
            {
                this.AlertsService.AlertConfirmed(model, true);
            }
        }

        public void DenyAlert_OnClicked(AlertsModel model)
        {
            this.AlertsService.RemoveAlert(model.Id);
            if (model.ConfirmDialog)
            {
                this.AlertsService.AlertConfirmed(model, false);
            }
        }

        public void Dispose()
        {
            this.AlertsService.OnAlert -= OnAlertExecute;
        }

    }
}
