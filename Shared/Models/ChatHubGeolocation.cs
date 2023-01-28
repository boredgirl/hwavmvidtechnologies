using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.ChatHubs.Models
{
    public class ChatHubGeolocation : ChatHubBaseModel
    {

        public int ChatHubConnectionId { get; set; }

        public string state { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }
        public double altitude { get; set; }
        public double altitudeaccuracy { get; set; }
        public double accuracy { get; set; }
        public double heading { get; set; }
        public double speed { get; set; }

        [NotMapped] public virtual ChatHubConnection Connection { get; set; }

    }
}
