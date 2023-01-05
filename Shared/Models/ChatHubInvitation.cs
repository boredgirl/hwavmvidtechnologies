using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.ChatHubs.Models
{
    public class ChatHubInvitation : ChatHubBaseModel
    {

        public int RoomId { get; set; }
        public string Hostname { get; set; }

        public int ChatHubUserId { get; set; }
        [NotMapped] public virtual ChatHubUser User { get; set; }

    }
}
