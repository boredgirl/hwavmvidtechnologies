using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Oqtane.ChatHubs.Models
{

    public class ChatHubUser : User
    {

        public int? FrameworkUserId { get; set; }
        public string UserType { get; set; }

        [NotMapped] public bool UserlistItemCollapsed { get; set; }
        [NotMapped] public virtual IList<ChatHubRoomChatHubUser> UserRooms { get; set; }
        [NotMapped] public virtual IList<ChatHubConnection> Connections { get; set; }
        [NotMapped] public virtual ChatHubSettings Settings { get; set; }
        [NotMapped] public virtual ICollection<ChatHubIgnore> Ignores { get; set; }
        [NotMapped] public virtual IList<ChatHubInvitation> Invitations { get; set; }
        [NotMapped] public virtual IList<ChatHubDevice> Devices { get; set; }

    }
}