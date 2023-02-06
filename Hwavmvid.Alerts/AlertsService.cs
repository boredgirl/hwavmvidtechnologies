using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Hwavmvid.Alerts
{
    public class AlertsService : IAlertsService
    {

        public event Action<string, string, PositionType, bool, string> OnAlert;

        /// <summary>
        ///    Returns a dynamic object with the values 'model' as a alertmodel and 'confirmed' as a boolean.
        /// </summary>
        public event EventHandler<dynamic> OnAlertConfirmed;

        public List<AlertsModel> Alerts { get; set; } = new List<AlertsModel>();

        public void NewAlert(string message, string heading = "[Javascript Application]", PositionType position = PositionType.Absolute, bool confirmDialog = false, string id = null)
        {
            this.OnAlert?.Invoke(message, heading, position, confirmDialog, id);
        }

        public void AddAlert(AlertsModel model)
        {
            if(!this.Alerts.Any(item => item.Id == model.Id))
            {
                this.Alerts.Add(model);
            }
        }

        public void RemoveAlert(string id)
        {
            AlertsModel item = this.Alerts.FirstOrDefault(item => item.Id == id);
            if(item != null)
            {
                this.Alerts.Remove(item);
            }
        }

        public void AlertConfirmed(AlertsModel model, bool confirmed)
        {
            dynamic obj = new ExpandoObject();
            obj.confirmed = confirmed;
            obj.model = model;

            this.OnAlertConfirmed?.Invoke(this, obj);
        }

    }
}
