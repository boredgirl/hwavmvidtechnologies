using Oqtane.ChatHubs.Models;

namespace Oqtane.ChatHubs.Repository
{
    public static class ChathubClientModelsExtensionMethods
    {
        public static ChatHubRoom ClientModel(this ChatHubRoom obj)
        {
            ChatHubRoom room = new ChatHubRoom()
            {
                Id = obj.Id,
                ModuleId = obj.ModuleId,
                CreatorId = obj.CreatorId,
                OneVsOneId = obj.OneVsOneId,
                Status = obj.Status,
                Content = obj.Content,
                ImageUrl = obj.ImageUrl,
                BackgroundColor = obj.BackgroundColor,
                Title = obj.Title,
                Type = obj.Type,
                SnapshotUrl = obj.SnapshotUrl,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return room;
        }
        public static ChatHubUser ClientModel(this ChatHubUser obj)
        {
            ChatHubUser user = new ChatHubUser()
            {
                UserId = obj.UserId,
                Username = obj.Username,
                DisplayName = obj.DisplayName,
                Email = obj.Email,
                LastIPAddress = obj.LastIPAddress,
                LastLoginOn = obj.LastLoginOn,
                PhotoFileId = obj.PhotoFileId,
                UserType = obj.UserType,
                DeletedBy = obj.DeletedBy,
                DeletedOn = obj.DeletedOn,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return user;
        }
        public static ChatHubConnection ClientModel(this ChatHubConnection obj)
        {
            ChatHubConnection connection = new ChatHubConnection()
            {
                Id = obj.Id,
                ConnectionId = obj.ConnectionId,
                ChatHubUserId = obj.ChatHubUserId,
                Status = obj.Status,
                IpAddress = obj.IpAddress,
                UserAgent = obj.UserAgent,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return connection;
        }
        public static ChatHubCam ClientModel (this ChatHubCam obj) {

            ChatHubCam cam = new ChatHubCam()
            {
                Id = obj.Id,
                Status = obj.Status,
                ChatHubConnectionId = obj.ChatHubConnectionId,
                ChatHubRoomId = obj.ChatHubRoomId,
                TotalVideoSequences = obj.TotalVideoSequences,
                VideoUrl = obj.VideoUrl,
                VideoUrlExtension = obj.VideoUrlExtension,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return cam;
        }
        public static ChatHubSettings ClientModel(this ChatHubSettings obj)
        {

            ChatHubSettings settings = new ChatHubSettings()
            {
                Id = obj.Id,
                ChatHubUserId = obj.ChatHubUserId,
                MessageColor = obj.MessageColor,
                UsernameColor = obj.MessageColor,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return settings;
        }
        public static ChatHubMessage ClientModel(this ChatHubMessage obj)
        {
            ChatHubMessage message = new ChatHubMessage()
            {
                Id = obj.Id,
                ChatHubUserId = obj.ChatHubUserId,
                ChatHubRoomId = obj.ChatHubRoomId,
                Type = obj.Type,
                Content = obj.Content,                
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return message;
        }
        public static ChatHubIgnore ClientModel(this ChatHubIgnore obj)
        {
            ChatHubIgnore ignore = new ChatHubIgnore()
            {
                Id = obj.Id,
                ChatHubUserId = obj.ChatHubUserId,
                ChatHubIgnoredUserId = obj.ChatHubIgnoredUserId,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return ignore;
        }

        public static ChatHubDevice ClientModel(this ChatHubDevice obj)
        {
            ChatHubDevice device = new ChatHubDevice()
            {
                Id = obj.Id,
                ChatHubUserId = obj.ChatHubUserId,
                UserAgent = obj.UserAgent,
                Type = obj.Type,
                DefaultDeviceId = obj.DefaultDeviceId,
                DefaultDeviceName = obj.DefaultDeviceName,
                CreatedBy = obj.CreatedBy,
                CreatedOn = obj.CreatedOn,
                ModifiedBy = obj.ModifiedBy,
                ModifiedOn = obj.ModifiedOn,
            };

            return device;
        }
    }
}
