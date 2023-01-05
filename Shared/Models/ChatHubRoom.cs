using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.ChatHubs.Models
{
    public class ChatHubRoom : ChatHubBaseModel
    {

        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string BackgroundColor { get; set; }
        public string ImageUrl { get; set; }
        public string SnapshotUrl { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string OneVsOneId { get; set; }
        public int CreatorId { get; set; }
        
        [NotMapped] public string MessageInput { get; set; }
        [NotMapped] public int UnreadMessages { get; set; }
        [NotMapped] public bool ShowUserlist { get; set; }
        [NotMapped] public int UsersLength { get; set; }
        [NotMapped] public int ViewersLength { get; set; }
        [NotMapped] public bool Broadcasting { get; set; }
        [NotMapped] public bool Motiondetection { get; set; } = false;
        [NotMapped] public int Motiondetectionfluctation { get; set; } = 1000;

        [NotMapped] public virtual ICollection<ChatHubMessage> Messages { get; set; }
        [NotMapped] public virtual ICollection<ChatHubRoomChatHubUser> RoomUsers { get; set; }
        [NotMapped] public virtual List<ChatHubUser> Users { get; set; } = new List<ChatHubUser>();
        [NotMapped] public virtual ICollection<ChatHubRoomChatHubModerator> RoomModerators { get; set; }
        [NotMapped] public virtual List<ChatHubModerator> Moderators { get; set; }
        [NotMapped] public virtual ICollection<ChatHubRoomChatHubWhitelistUser> RoomWhitelistUsers { get; set; }
        [NotMapped] public virtual List<ChatHubWhitelistUser> WhitelistUsers { get; set; }
        [NotMapped] public virtual ICollection<ChatHubRoomChatHubBlacklistUser> RoomBlacklistUsers { get; set; }
        [NotMapped] public virtual List<ChatHubBlacklistUser> BlacklistUsers { get; set; }
        [NotMapped] public ICollection<ChatHubWaitingRoomItem> WaitingRoomItems { get; set; } = new List<ChatHubWaitingRoomItem>();
        [NotMapped] public virtual ChatHubUser Creator { get; set; }
        [NotMapped] public virtual ICollection<ChatHubCam> Cams { get; set; }
        [NotMapped] public virtual IList<ChatHubViewer> Viewers { get; set; } = new List<ChatHubViewer>();
        [NotMapped] public virtual IList<ChatHubDevice> Devices { get; set; }

    }
}
