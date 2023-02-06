using System;

namespace Hwavmvid.Alerts
{
    public interface IAlertsService
    {

        void NewAlert(string message, string heading, PositionType position = PositionType.Fixed, bool confirmDialog = false, string id = null);

        void AddAlert(AlertsModel model);

        void RemoveAlert(string id);

    }
}
