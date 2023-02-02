using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using Oqtane.Shared;
using Microsoft.EntityFrameworkCore;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Repository;
using Oqtane.ChatHubs.Commands;
using Oqtane.Modules;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using BlazorVideo;
using BlazorVideoPlayer;
using Microsoft.Extensions.Caching.Memory;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Hwavmvid.Motiondetection;
using System.Text;
using System.Text.Json;

namespace Oqtane.ChatHubs.Hubs
{

    [AllowAnonymous]
    public class ChatHub : Hub, IService
    {

        private readonly ChatHubContext _db;
        private readonly IUserRepository userRepository;
        private readonly ChatHubRepository chatHubRepository;
        private readonly ChatHubService chatHubService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRoleRepository roles;
        private readonly IUserRoleRepository userRoles;
        private readonly IWebHostEnvironment webHostEnvironment;
        private IMemoryCache cache { get; set; }

        public ChatHub(
            ChatHubContext dbContext,
            IUserRepository userRepository,
            ChatHubRepository chatHubRepository,
            ChatHubService chatHubService,
            UserManager<IdentityUser> identityUserManager,
            IRoleRepository roles,
            IUserRoleRepository userRoles,
            IWebHostEnvironment webHostEnvironment,
            IMemoryCache cache
            )
        {
            this._db = dbContext;
            this.userRepository = userRepository;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.userManager = identityUserManager;
            this.roles = roles;
            this.userRoles = userRoles;
            this.webHostEnvironment = webHostEnvironment;
            this.cache = cache;
        }

        private async Task<ChatHubUser> GetChatHubUserAsync()
        {
            ChatHubUser user = await this.IdentifyUser();
            if (user == null)
            {
                user = await this.IdentifyGuest(Context.ConnectionId);
            }

            if (user == null)
            {
                throw new HubException("No valid user found.");
            }

            return user;
        }
        public async Task<ChatHubUser> IdentifyGuest(string connectionId)
        {
            ChatHubConnection connection = chatHubRepository.Connections().Include(item => item.User).ByConnectionId(connectionId);
            if (connection != null)
            {
                var guest = await this.chatHubRepository.GetUserByIdAsync(connection.User.UserId);
                if(guest.UserType == ChatHubUserType.Guest.ToString())
                {
                    return guest;
                }
            }

            return null;
        }
        public async Task<ChatHubUser> IdentifyUser()
        {
            var username = Context.User.Identity.Name;
            if (Context.User.Identity.IsAuthenticated)
            {
                ChatHubUser chatHubUser = await this.chatHubRepository.GetUserByUserNameAsync(username);
                if (chatHubUser == null)
                {
                    User user = this.userRepository.GetUser(username);
                    if (user != null)
                    {
                        var newChatHubUser = new ChatHubUser()
                        {
                            //UserId = user.UserId,
                            Username = Constants.ChatHubConstants.ChatHubUserPrefix + user.Username,
                            DisplayName = user.DisplayName,
                            Email = user.Email,
                            PhotoFileId = user.PhotoFileId,
                            LastLoginOn = user.LastLoginOn,
                            LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                            CreatedBy = user.Username,
                            CreatedOn = user.CreatedOn,
                            ModifiedBy = user.ModifiedBy,
                            ModifiedOn = user.ModifiedOn,
                            DeletedBy = user.DeletedBy,
                            DeletedOn = user.DeletedOn,
                            IsDeleted = user.IsDeleted,
                            IsAuthenticated = user.IsAuthenticated,
                            SiteId = user.SiteId,
                            
                            FrameworkUserId = user.UserId,
                            UserType = ChatHubUserType.User.ToString(),
                        };

                        newChatHubUser = this.chatHubRepository.AddUser(newChatHubUser);
                        var userRole = new UserRole()
                        {
                            UserId = newChatHubUser.UserId,
                            RoleId = 3,
                            EffectiveDate = null,
                            ExpiryDate = null,
                        };
                        this.userRoles.AddUserRole(userRole);

                        return newChatHubUser;
                    }
                }

                return chatHubUser;
            }

            return null;
        }

        private async Task<ChatHubUser> OnConnectedGuest()
        {
            string guestname = null;
            guestname = Context.GetHttpContext().Request.Query["guestname"];
            guestname = guestname.Trim();

            if (string.IsNullOrEmpty(guestname) || !this.IsValidGuestUsername(guestname))
            {
                guestname = "guest";
            }

            string username = this.CreateUsername(guestname);
            string displayname = this.CreateDisplaynameFromUsername(username);

            if (await this.chatHubRepository.GetUserByDisplayNameAsync(displayname) != null)
            {
                throw new HubException("Displayname already in use. Goodbye.");
            }

            string email = "noreply@anyways.tv";
            //string password = "§PasswordPolicy42";

            ChatHubUser chatHubUser = new ChatHubUser()
            {
                //UserId = user.UserId,
                Username = Constants.ChatHubConstants.ChatHubUserPrefix + username,
                DisplayName = displayname,
                Email = email,
                PhotoFileId = null,
                LastLoginOn = DateTime.UtcNow,
                LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                CreatedBy = Constants.ChatHubConstants.ChatHubUserPrefix + username,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = string.Empty,
                ModifiedOn = DateTime.MinValue,
                DeletedBy = null,
                DeletedOn = null,
                IsDeleted = false,

                FrameworkUserId = null,
                UserType = ChatHubUserType.Guest.ToString(),
            };

            chatHubUser = this.chatHubRepository.AddUser(chatHubUser);
            var userRole = new UserRole()
            {
                UserId = chatHubUser.UserId,
                RoleId = 1,
                EffectiveDate = null,
                ExpiryDate = null,
            };
            this.userRoles.AddUserRole(userRole);

            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
            };
            ChatHubConnection = this.chatHubRepository.AddConnection(ChatHubConnection);

            ChatHubSettings ChatHubSetting = new ChatHubSettings()
            {
                UsernameColor = "#7744aa",
                MessageColor = "#44aa77",
                ChatHubUserId = chatHubUser.UserId
            };
            ChatHubSetting = this.chatHubRepository.AddSetting(ChatHubSetting);

            return chatHubUser;
        }
        private ChatHubUser OnConnectedUser(ChatHubUser chatHubUser)
        {
            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = ChatHubConnectionStatus.Active.ToString(),
                CreatedBy = chatHubUser.Username,
            };
            ChatHubConnection = this.chatHubRepository.AddConnection(ChatHubConnection);            

            ChatHubSettings ChatHubSetting = this.chatHubRepository.GetSettingByUser(chatHubUser);
            if(ChatHubSetting == null)
            {
                ChatHubSetting = new ChatHubSettings()
                {
                    UsernameColor = "#7744aa",
                    MessageColor = "#44aa77",
                    ChatHubUserId = chatHubUser.UserId,
                    CreatedBy = chatHubUser.Username,
                };
                ChatHubSetting = this.chatHubRepository.AddSetting(ChatHubSetting);
            }

            return chatHubUser;
        }
        [AllowAnonymous]
        public override async Task OnConnectedAsync()
        {
            HttpContext httpContext = Context.GetHttpContext();
            string moduleId = httpContext.Request.Headers["moduleid"];
            string platform = httpContext.Request.Headers["platform"];

            //await this.chatHubRepository.UpdateUserColumnAsync();
            ChatHubUser user = await this.IdentifyUser();
            if (user != null)
            {
                user = this.OnConnectedUser(user);
            }
            else
            {
                user = await this.OnConnectedGuest();
            }

            ChatHubUser chatHubUserClientModel = await this.chatHubService.CreateChatHubUserClientModelWithConnections(user);
            await Clients.Clients(chatHubUserClientModel.Connections.Select(item => item.ConnectionId).ToArray<string>()).SendAsync("OnUpdateConnectedUser", chatHubUserClientModel);         

            var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(user);
            var creatorRooms = await this.chatHubRepository.GetRoomsByCreator(user.UserId).ToListAsync();
            foreach (var room in creatorRooms)
            {
                await this.UpdateRoomCreator(room, exceptConnectionIds);
            }

            await base.OnConnectedAsync();
        }

        [AllowAnonymous]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            var contextUser = await this.GetChatHubUserAsync();
            var contextConnection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            var contextConnections = await this.chatHubRepository.GetConnectionsByUserId(contextUser.UserId).Active().ToListAsync();
            var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
            var rooms = await chatHubRepository.GetRoomsByUser(contextUser).Enabled().ToListAsync();

            foreach (var room in rooms)
            {
                List<ChatHubCam> cams = await this.chatHubRepository.GetCamsByConnectionId(contextConnection.Id).ToListAsync();
                foreach (var cam in cams)
                {
                    cam.Status = ChatHubCamStatus.Archived.ToString();
                    await this.chatHubRepository.UpdateCam(cam);
                }

                if (contextConnections.Count() == 1)
                {
                    var userClientModel = this.chatHubService.CreateChatHubUserClientModel(contextUser);
                    await Clients.Group(room.Id.ToString()).SendAsync("RemoveUser", userClientModel, room.Id.ToString());
                }

                await this.SendGroupNotification(string.Format("{0} disconnected from chat with client device {1}.", contextUser.DisplayName, Context.ConnectionId), room.Id, contextUser, ChatHubMessageType.Connect_Disconnect);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());
            }

            if (!Context.User.HasClaim(ClaimTypes.Role, RoleNames.Registered) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host))
            {
                var roomsByCreator = this.chatHubRepository.GetRoomsByCreator(contextUser.UserId);
                foreach (var room in await roomsByCreator.ToListAsync())
                {
                    room.Status = ChatHubRoomStatus.Archived.ToString();
                    await this.chatHubRepository.UpdateRoomStatus(room);
                }
            }

            contextConnection.Status = ChatHubConnectionStatus.Archived.ToString();
            await chatHubRepository.UpdateConnection(contextConnection);

            ChatHubUser chatHubUserClientModel = await this.chatHubService.CreateChatHubUserClientModelWithConnections(contextUser);
            await Clients.Clients(contextConnections.Select(item => item.ConnectionId)).SendAsync("OnUpdateConnectedUser", chatHubUserClientModel);

            var creatorRooms = await this.chatHubRepository.GetRoomsByCreator(contextUser.UserId).ToListAsync();
            foreach(var room in creatorRooms)
            {
                await this.UpdateRoomCreator(room, exceptConnectionIds);
            }

            await base.OnDisconnectedAsync(exception);
        }

        [AllowAnonymous]
        public async Task Init()
        {

            string moduleId = Context.GetHttpContext().Request.Query["moduleid"];
            var contextUser = await this.GetChatHubUserAsync();

            var rooms = this.chatHubRepository.GetRoomsByUser(contextUser).Public().Enabled().ToList();
            rooms.AddRange(this.chatHubRepository.GetRoomsByUser(contextUser).Private().Enabled().ToList());

            if (Context.User.Identity.IsAuthenticated)
            {
                rooms.AddRange(this.chatHubRepository.GetRoomsByUser(contextUser).Protected().Enabled().ToList());
            }
            
            foreach (var room in rooms)
            {
                await this.EnterChatRoom(room.Id);
            }
        }

        [AllowAnonymous]
        public async Task EnterChatRoom(int roomId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var contextConnection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            var contextConnections = await this.chatHubRepository.GetConnectionsByUserId(contextUser.UserId).Active().ToListAsync();
            var contextRoom = await chatHubRepository.GetRoomById(roomId);

            IList<ChatHubMessage> recentMessagesClientModels = new List<ChatHubMessage>();

            if (contextRoom == null)
                throw new HubException("This room does not exist anymore.");

            if(contextRoom.Status == ChatHubRoomStatus.Archived.ToString())
            {
                if (!Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host))
                {
                    throw new HubException("You cannot enter an archived room.");
                }
            }

            if(contextRoom.Public() || contextRoom.Protected())
            {
                if (this.chatHubService.IsBlacklisted(contextRoom, contextUser))
                {
                    throw new HubException("You have been added to blacklist for this room.");
                }
            }

            if(contextRoom.Protected())
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    throw new HubException("This room is for authenticated user only.");
                }
            }

            if (contextRoom.Private())
            {
                if (!this.chatHubService.IsWhitelisted(contextRoom, contextUser))
                {
                    if(contextRoom.CreatorId != contextUser.UserId)
                    {
                        await this.AddWaitingRoomItem(contextUser, contextRoom);
                        throw new HubException("No valid private room connection. You have been added to waiting list.");
                    }
                }
            }

            if (contextRoom.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(contextRoom, contextUser))
                {
                    throw new HubException("No valid one vs one room id.");
                }

                var recentMessages = this.chatHubRepository.Messages().Where(item => item.ChatHubRoomId == contextRoom.Id).OrderByDescending(item => item.CreatedOn).Take(40).ToList();
                recentMessagesClientModels = recentMessages != null && recentMessages.Any() ? recentMessages.Select(msgitem => {

                        var msguser = this.chatHubRepository.Users().FirstOrDefault(useritem => useritem.UserId == msgitem.ChatHubUserId);
                        return this.chatHubService.CreateChatHubMessageClientModel(msgitem, msguser);
                    }).ToList() : new List<ChatHubMessage>();
            }

            if (contextRoom.Public() || contextRoom.Protected() || contextRoom.Private() || contextRoom.OneVsOne())
            {

                ChatHubCam cam = await this.chatHubRepository.GetCam(roomId, contextConnection.Id);
                if (cam == null)
                {
                    cam = new ChatHubCam()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubConnectionId = contextConnection.Id,
                        Status = ChatHubCamStatus.Inactive.ToString(),
                        VideoUrl = String.Empty,
                        VideoUrlExtension = String.Empty,
                        CreatedBy = contextUser.Username,
                    };

                    cam = await this.chatHubRepository.AddCam(cam);
                    var camClientModel = cam.ClientModel();
                    await Clients.Group(contextRoom.Id.ToString()).SendAsync("AddCam", this.chatHubService.CreateChatHubCamClientModel(camClientModel), contextRoom.Id.ToString());
                }

                ChatHubRoomChatHubUser room_user = new ChatHubRoomChatHubUser()
                {
                    ChatHubRoomId = contextRoom.Id,
                    ChatHubUserId = contextUser.UserId,
                };
                chatHubRepository.AddRoomUser(room_user);

                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(contextRoom);
                chatHubRoomClientModel.Messages = recentMessagesClientModels;

                foreach (var connection in contextConnections)
                {
                    await Groups.AddToGroupAsync(connection.ConnectionId, contextRoom.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("AddRoom", chatHubRoomClientModel);
                }

                await this.SendGroupNotification(string.Format("{0} entered chat room with client device {1}.", contextUser.DisplayName, Context.ConnectionId), contextRoom.Id, contextUser, ChatHubMessageType.Enter_Leave);
            }
        }
        [AllowAnonymous]
        public async Task LeaveChatRoom(int roomId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var contextConnection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            var contextConnections = await this.chatHubRepository.GetConnectionsByUserId(contextUser.UserId).Active().ToListAsync();
            var contextRoom = await chatHubRepository .GetRoomById(roomId);

            if (contextRoom == null)
                throw new HubException("This room does not exist anymore.");

            if (!this.chatHubRepository.GetUsersByRoom(contextRoom).Any(item => item.UserId == contextUser.UserId))
            {
                throw new HubException("User already left room.");
            }

            if (contextRoom.Public() || contextRoom.Protected())
            {
                if (this.chatHubService.IsBlacklisted(contextRoom, contextUser))
                {
                    //throw new HubException("You have been added to blacklist for this room.");
                }
            }

            if (contextRoom.Protected())
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    //throw new HubException("This room is for authenticated user only.");
                }
            }

            if (contextRoom.Private())
            {
                if (!this.chatHubService.IsWhitelisted(contextRoom, contextUser))
                {
                    //throw new HubException("No valid private room connection.");
                }
            }

            if (contextRoom.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(contextRoom, contextUser))
                {
                    throw new HubException("No valid one vs one room id.");
                }
            }

            if (contextRoom.Public() || contextRoom.Protected() || contextRoom.Private() || contextRoom.OneVsOne())
            {
                this.chatHubRepository.DeleteRoomUser(roomId, contextUser.UserId);
                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(contextRoom);

                foreach (var connection in contextConnections)
                {
                    await Groups.RemoveFromGroupAsync(connection.ConnectionId, contextRoom.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("RemoveRoom", chatHubRoomClientModel);
                }
                
                await this.SendGroupNotification(string.Format("{0} left chat room with client device {1}.", contextUser.DisplayName, Context.ConnectionId), contextRoom.Id, contextUser, ChatHubMessageType.Enter_Leave);              
            }
        }

        [AllowAnonymous]
        public async Task SendMessage(string message, int roomId)
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await chatHubRepository .GetRoomById(roomId);

            if (contextRoom == null)
                throw new HubException("This room does not exist anymore.");

            if (await ExecuteCommandManager(contextUser, message, roomId, Convert.ToInt32(moduleId)))
            {
                return;
            }

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = contextUser.UserId,
                Content = message ?? string.Empty,
                Type = this.GetMessageType(),
            };
            await this.chatHubRepository.AddMessage(chatHubMessage);
            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, contextUser);

            var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
            await Clients.GroupExcept(roomId.ToString(), exceptConnectionIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }
        private async Task<bool> ExecuteCommandManager(ChatHubUser chatHubUser, string message, int roomId, int moduleId)
        {
            var commandManager = new CommandManager(Context.ConnectionId, roomId, moduleId, chatHubUser, this, chatHubService, chatHubRepository, userManager);
            return await commandManager.TryHandleCommand(message);
        }
        [AllowAnonymous]
        public async Task SendCommandMetaDatas(int roomId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var contextConnection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            var contextConnections = await this.chatHubRepository.GetConnectionsByUserId(contextUser.UserId).Active().ToListAsync();

            List<string> callerUserRoles = new List<string>() { RoleNames.Everyone };

            if (Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
            {
                callerUserRoles.Add(RoleNames.Admin);
            }

            List<ChatHubCommandMetaData> commandMetaDatas = CommandManager.GetCommandsMetaDataByUserRole(callerUserRoles.ToArray()).ToList();
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = contextUser.UserId,
                Content = string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Commands),
                CommandMetaDatas = commandMetaDatas
            };
            await this.chatHubRepository.AddMessage(chatHubMessage);
            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, contextUser);

            await Clients.Clients(contextConnections.Select(c => c.ConnectionId).ToArray<string>()).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        public async Task SendClientNotification(string message, int roomId, string connectionId, ChatHubUser targetUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = targetUser.UserId,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            await this.chatHubRepository.AddMessage(chatHubMessage);
            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, targetUser);

            await Clients.Client(connectionId).SendAsync("AddMessage", chatHubMessageClientModel);
        }
        public async Task SendGroupNotification(string message, int roomId, ChatHubUser contextUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = contextUser.UserId,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            await this.chatHubRepository.AddMessage(chatHubMessage);
            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, contextUser);

            var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
            await Clients.GroupExcept(roomId.ToString(), exceptConnectionIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        [AllowAnonymous]
        public async Task<IList<ChatHubViewer>[]> GetChatHubViewers(List<int> roomIds)
        {
            try
            {
                IList<ChatHubViewer>[] viewerListArray = new IList<ChatHubViewer>[roomIds.Count()];
                foreach (var item in roomIds.Select((roomId, index) => new { roomId = roomId, index = index }))
                {
                    viewerListArray[item.index] = await this.chatHubRepository.GetViewersByRoomIdAsync(item.roomId);
                }

                return viewerListArray;
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }            
        }

        [AllowAnonymous]
        public async Task StartCam(int roomId, int camId)
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await chatHubRepository.GetRoomById(roomId);

            if (contextRoom == null)
                throw new HubException("This room does not exist anymore.");

            ChatHubConnection connection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            ChatHubCam cam = await this .chatHubRepository.GetCamById(camId);
            if(cam != null)
            {
                cam.Status = contextUser.UserId == contextRoom.CreatorId ? ChatHubCamStatus.Broadcasting.ToString() : ChatHubCamStatus.Streaming.ToString();
                await this.chatHubRepository.UpdateCam(cam);
            }

            if (contextUser.UserId == contextRoom.CreatorId)
            {
                var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
                await this.UpdateRoomCreator(contextRoom, exceptConnectionIds);
            }

            if(cam.Status == ChatHubCamStatus.Streaming.ToString())
            {
                var contextRoomBroadcastingCams = this.chatHubRepository.GetCamsByRoomId(contextRoom.Id).Broadcasting();
                var userRooms = await this.chatHubRepository.GetRoomsByUser(contextUser).FilterByModuleId(Int32.Parse(moduleId)).Where(room => room.CreatorId == contextUser.UserId && room.Status == ChatHubRoomStatus.Enabled.ToString()).ToListAsync();
                foreach (var userRoom in userRooms)
                {
                    var broadcastingCams = await this.chatHubRepository.GetCamsByRoomId(userRoom.Id).Broadcasting().ToListAsync();
                    if (broadcastingCams != null && broadcastingCams.Any())
                    {
                        var streamingCams = await this.chatHubRepository.GetCamsByRoomId(userRoom.Id).Streaming().ToListAsync();
                        foreach (var streamingCam in streamingCams)
                        {                            
                            var matched = contextRoomBroadcastingCams.Any(cam => cam.ChatHubConnectionId == streamingCam.ChatHubConnectionId);
                            if (matched)
                            {
                                var streamingConnection = this.chatHubRepository.Connections().Include(item => item.User).ById(streamingCam.ChatHubConnectionId);
                                var streamingUser = streamingConnection.User;
                                string message = $"{ contextUser.DisplayName } matched { streamingUser.DisplayName }.";
                                await Clients.Clients(new string[] { streamingConnection.ConnectionId, Context.ConnectionId }).SendAsync("Matched", message);
                            }
                        }
                    }
                }
            }
        }
        [AllowAnonymous]
        public async Task StopCam(int roomId, int camId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await this.chatHubRepository.GetRoomById(roomId);

            if (contextRoom == null)
                throw new HubException("This room does not exist anymore.");

            ChatHubConnection connection = this.chatHubRepository.Connections().ByConnectionId(Context.ConnectionId);
            ChatHubCam cam = await this.chatHubRepository.GetCamById(camId);
            if (cam != null)
            {
                cam.Status = ChatHubCamStatus.Inactive.ToString();
                await this.chatHubRepository.UpdateCam(cam);
            }

            if(contextUser.UserId == contextRoom.CreatorId)
            {
                var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
                await this.UpdateRoomCreator(contextRoom, exceptConnectionIds);
            }
        }

        [AllowAnonymous]
        public async Task UploadDataUri(string dataUri, string roomId, string camId, bool motiondetection, int motiondetectionfluctation)
        {

            var contextUser = await this.GetChatHubUserAsync();
            var contextConnections = await this.chatHubRepository.GetConnectionsByUserId(contextUser.UserId).Active().ToListAsync();
            var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);

            try
            {
                string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];

                string fileExtension = ".webm";
                string filename = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string directoryPath = Path.Combine(webRootPath, Constants.ChatHubConstants.UploadVideosPath, contextUser.Username, moduleId, roomId.ToString(), camId);
                string fullPath = Path.Combine(directoryPath, filename + fileExtension);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                if (!System.IO.File.Exists(fullPath))
                {
                    dataUri = dataUri.Split("base64,")[1];
                    byte[] bytes = Convert.FromBase64String(dataUri);
                    await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                    #region video converting
                    /*
                    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./wwwroot/Modules/Oqtane.ChatHubs", TemporaryFilesFolder = "/tmp" });
                    await FFMpegArguments
                            .FromPipeInput(new StreamPipeSource(new MemoryStream(Convert.FromBase64String(dataUri))))
                            .OutputToFile(fullPath, false, options => options
                            .WithVideoCodec(VideoCodec.LibVpx)
                            .WithAudioCodec(AudioCodec.LibVorbis)
                            //.WithConstantRateFactor(21)
                            //.WithVariableBitrate(4)
                            .WithVideoFilters(filterOptions => filterOptions
                            .Scale(VideoSize.Ld))
                            .WithFastStart())
                            .ProcessAsynchronously();
                    */
                    #endregion

                    ChatHubCamSequence sequence = new ChatHubCamSequence()
                    {
                        ChatHubCamId = Convert.ToInt32(camId),
                        Filename = filename,
                        FilenameExtension = fileExtension,
                    };
                    await this.chatHubRepository.AddCamSequence(sequence);

                    foreach (var c in contextConnections)
                    {
                        exceptConnectionIds.Add(c.ConnectionId);
                    }

                    //var base64Uri = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(fullPath));
                    await Clients.GroupExcept(roomId, exceptConnectionIds).SendAsync("DownloadBytes", dataUri, roomId, camId);

                    #region video converter hwavmvid opus vp17
                    /*
                    var list = bytes.ToList();
                    var index = list.IndexOf(174);
                    list = list.Skip(index).ToList<byte>();
                    List<List<byte>> videoelements = new List<List<byte>>();
                    foreach (var item in list) {

                        if (item == 174)
                            videoelements.Add(new List<byte>());

                        videoelements.Last().Add(item);
                    }
                        
                    string output = string.Empty;
                    int result = 0;
                    foreach (var items in videoelements)
                    {
                        var byterate = items.ToArray<byte>().MotiondetectionByterate(1);
                        output += string.Concat(byterate, ";");
                        result += byterate;
                    }

                    string lowercasehexstr = string.Empty;
                    foreach (byte item in bytes)
                    {
                        lowercasehexstr += item.ToString("x2");
                    }

                    var uppercasehexstr = Convert.ToHexString(bytes);
                    */
                    #endregion
                    var superuser = Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host) || Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin);
                    if (motiondetection && (superuser))
                    {
                        var byterate = bytes.MotiondetectionByterate();
                        if (byterate >= motiondetectionfluctation)
                            await this.SendClientNotification(string.Concat("Motiondetection fluctation: ", byterate), Convert.ToInt32(roomId), Context.ConnectionId, contextUser, ChatHubMessageType.System);
                    }
                    
                }
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task<BlazorVideoPlayerApiItem> DowloadDataUri(string roomId, string camId, string lastSequenceId, int sliderCurrentValue, bool sliderValueChanged)
        {
            try
            {
                string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
                var contextUser = await this.GetChatHubUserAsync();
                var contextCam = await this.chatHubRepository.GetCamById(Convert.ToInt32(camId));
                ChatHubCamSequence camSequence = null;
                BlazorVideoPlayerApiItem apiItem = new BlazorVideoPlayerApiItem();

                if (contextCam != null)
                {
                    if (string.IsNullOrEmpty(lastSequenceId) && !sliderValueChanged)
                    {
                        camSequence = this.chatHubRepository.CamSequences().FirstOrDefault(item => item.ChatHubCamId == contextCam.Id);
                        apiItem.LastSequence = camSequence.Id.ToString();
                        apiItem.SliderCurrentValue = 1;
                        apiItem.SliderValueChanged = sliderValueChanged;
                    }
                    else if (sliderValueChanged)
                    {
                        var items = this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == contextCam.Id).ToList();
                        if (items.Count() - 1 >= sliderCurrentValue)
                        {
                            camSequence = items[sliderCurrentValue];
                            apiItem.LastSequence = camSequence.Id.ToString();
                            apiItem.SliderCurrentValue = sliderCurrentValue + 1;
                            apiItem.SliderValueChanged = true;
                            apiItem.TotalSequences = items.Count();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        var camSequences = this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == contextCam.Id).ToList();
                        var camSequencesOrdered = camSequences.OrderBy(item => item.Id).ToList();
                        var lastCamSequenceIndex = camSequencesOrdered.FindIndex(item => item.Id == Convert.ToInt32(lastSequenceId));
                        var targetCamSequenceIndex = lastCamSequenceIndex + 1;

                        if (camSequencesOrdered.Count() - 1 >= targetCamSequenceIndex)
                        {
                            camSequence = camSequencesOrdered[targetCamSequenceIndex];
                            apiItem.LastSequence = camSequence.Id.ToString();
                            apiItem.SliderCurrentValue = targetCamSequenceIndex + 1;
                            apiItem.SliderValueChanged = sliderValueChanged;
                            apiItem.TotalSequences = camSequences.Count();
                        }
                        else
                        {
                            return null;
                        }                        
                    }

                    if (camSequence != null)
                    {
                        string fileExtension = camSequence.FilenameExtension;
                        string filename = camSequence.Filename;
                        string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                        string directoryPath = Path.Combine(webRootPath, Constants.ChatHubConstants.UploadVideosPath, contextUser.Username, moduleId, roomId, camId);
                        string fullPath = Path.Combine(directoryPath, filename + fileExtension);

                        if (Directory.Exists(directoryPath))
                        {
                            if (System.IO.File.Exists(fullPath))
                            {
                                var base64Uri = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(fullPath));
                                apiItem.Base64DataUri = base64Uri;
                                return apiItem;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }

        [AllowAnonymous]
        public async Task UploadSnapshotUri(string imageUri, int roomId, BlazorVideoSnapshotActivatorType snapshotActivatorType)
        {            

            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await chatHubRepository.GetRoomById(roomId);

            try
            {
                string fileExtension = ".png";
                string filename = Guid.NewGuid().ToString();
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string directoryPath = Path.Combine(webRootPath, Constants.ChatHubConstants.UploadImagesPath);
                string fullPath = Path.Combine(directoryPath, filename + fileExtension);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                if (!System.IO.File.Exists(fullPath))
                {
                    await System.IO.File.WriteAllBytesAsync(fullPath, Convert.FromBase64String(imageUri));
                }

                ChatHubMessage chatHubMessage = new ChatHubMessage()
                {
                    ChatHubRoomId = contextRoom.Id,
                    ChatHubUserId = contextUser.UserId,
                    Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Image),
                    Content = "Snapshot",
                };
                chatHubMessage = await this.chatHubRepository.AddMessage(chatHubMessage);

                ChatHubPhoto chatHubPhoto = new ChatHubPhoto()
                {
                    ChatHubMessageId = chatHubMessage.Id,
                    Source = filename + fileExtension,
                    Size = 1000,
                    Thumb = filename + fileExtension,
                    Caption = "Snapshot: " + filename + fileExtension,
                    Message = chatHubMessage,
                    Width = 600,
                    Height = 400,
                };

                chatHubPhoto = this.chatHubRepository.AddPhoto(chatHubPhoto);
                chatHubMessage = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage, contextUser, new List<ChatHubPhoto>() { chatHubPhoto});

                if(snapshotActivatorType == BlazorVideoSnapshotActivatorType.LocalUser || snapshotActivatorType == BlazorVideoSnapshotActivatorType.RemoteUser)
                    await Clients.Group(contextRoom.Id.ToString()).SendAsync("AddMessage", chatHubMessage);

                contextRoom.SnapshotUrl = chatHubPhoto.Source;
                await this.chatHubRepository.UpdateRoom(contextRoom);
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }

        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredByUsers()
        {
            var contextUser = await this.GetChatHubUserAsync();

            IQueryable<ChatHubIgnore> ignoredByUsers = null;
            if (contextUser != null)
            {
                ignoredByUsers = this.chatHubRepository.GetIgnoredByUsers(contextUser);
            }

            IQueryable<ChatHubUser> chatHubUserClientModels = ignoredByUsers.Select(x =>

                this.chatHubService.CreateChatHubUserClientModel(x.User)
            );

            var list = await chatHubUserClientModels.ToListAsync();
            return list;
        }

        [AllowAnonymous]
        public async Task IgnoreUser(int targetUserId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var targetUser = await this.chatHubRepository.GetUserByIdAsync(targetUserId);

            if (contextUser != null && targetUser != null)
            {
                if (contextUser.UserId == targetUser.UserId)
                {
                    throw new HubException("Calling user cannot be target user.");
                }

                await this.chatHubService.IgnoreUser(contextUser, targetUser);
            }
        }
        [AllowAnonymous]
        public async Task UnignoreUser(int targetUserId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var targetUser = await this.chatHubRepository.GetUserByIdAsync(targetUserId);

            if (contextUser != null && targetUser != null)
            {
                this.chatHubRepository.DeleteIgnore(contextUser.UserId, targetUser.UserId);   
            }
        }

        [AllowAnonymous]
        public async Task AddModerator(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);
                var contextRoom = await this.chatHubRepository.GetRoomById(roomId);

                if (contextUser != null && targetUser != null)
                {
                    if (contextUser.UserId != contextRoom.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can add moderations.");
                    }

                    ChatHubModerator moderator = this.chatHubRepository.GetModerator(targetUser.UserId);
                    if(moderator == null)
                    {
                        moderator = new ChatHubModerator()
                        {
                            ChatHubUserId = targetUser.UserId,
                            ModeratorDisplayName = targetUser.DisplayName
                        };
                        moderator = this.chatHubRepository.AddModerator(moderator);
                    }

                    ChatHubRoomChatHubModerator room_moderator = new ChatHubRoomChatHubModerator()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubModeratorId = moderator.Id,
                    };
                    this.chatHubRepository.AddRoomModerator(room_moderator);

                    var targetModeratorClientModel = this.chatHubService.CreateChatHubModeratorClientModel(moderator);
                    await Clients.Group(roomId.ToString()).SendAsync("AddModerator", targetModeratorClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveModerator(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);
            
                if (contextUser != null && targetUser != null)
                {
                    var room = await this.chatHubRepository.GetRoomById(roomId);
                    if (contextUser.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can remove moderations.");
                    }

                    var moderator = this.chatHubRepository.GetModerator(targetUser.UserId);
                    this.chatHubRepository.DeleteRoomModerator(roomId, moderator.Id);

                    var targetModeratorClientModel = this.chatHubService.CreateChatHubModeratorClientModel(moderator);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveModerator", targetModeratorClientModel, roomId);
                }
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task AddWhitelistUser(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (contextUser != null && targetUser != null)
                {
                    if (contextUser.UserId == targetUser.UserId)
                    {
                        throw new HubException("Calling user cannot be target user.");
                    }

                    var room = await this.chatHubRepository.GetRoomById(roomId);
                    if (contextUser.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can add whitelist users.");
                    }

                    ChatHubWhitelistUser whitelistUser = this.chatHubRepository.AddWhitelistUser(targetUser);
                    ChatHubRoomChatHubWhitelistUser room_whitelistuser = new ChatHubRoomChatHubWhitelistUser()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubWhitelistUserId = whitelistUser.Id,
                    };
                    this.chatHubRepository.AddRoomWhitelistUser(room_whitelistuser);

                    var targetWhitelistUserClientModel = this.chatHubService.CreateChatHubWhitelistUserClientModel(whitelistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("AddWhitelistUser", targetWhitelistUserClientModel, roomId);
                    await this.SendClientNotification($"{targetUser.DisplayName} has been added to whitelist.", room.Id, Context.ConnectionId, contextUser.ClientModel(), ChatHubMessageType.System);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveWhitelistUser(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (contextUser != null && targetUser != null)
                {
                    var room = await this.chatHubRepository.GetRoomById(roomId);
                    if (contextUser.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can remove whitelist users.");
                    }

                    var whitelistUser = this.chatHubRepository.GetWhitelistUser(targetUser.UserId);
                    this.chatHubRepository.DeleteRooWhitelistUser(roomId, whitelistUser.Id);

                    var targetWhitelistUserClientModel = this.chatHubService.CreateChatHubWhitelistUserClientModel(whitelistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveWhitelistUser", targetWhitelistUserClientModel, roomId);
                    await this.SendClientNotification($"{targetUser.DisplayName} has been removed from whitelist.", room.Id, Context.ConnectionId, contextUser.ClientModel(), ChatHubMessageType.System);
                }
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task AddBlacklistUser(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (contextUser != null && targetUser != null)
                {
                    if(contextUser.UserId == targetUser.UserId)
                    {
                        throw new HubException("Calling user cannot be target user.");
                    }

                    var room = await this.chatHubRepository.GetRoomById(roomId);
                    var moderators = await this.chatHubRepository.GetModerators(room).ToListAsync();
                    if (contextUser.UserId != room.CreatorId && !moderators.Any(item => item.ChatHubUserId == contextUser.UserId) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators, moderators and administrators can add blacklist users.");
                    }

                    ChatHubBlacklistUser blacklistUser = this.chatHubRepository.AddBlacklistUser(targetUser);
                    ChatHubRoomChatHubBlacklistUser room_blacklistuser = new ChatHubRoomChatHubBlacklistUser()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubBlacklistUserId = blacklistUser.Id,
                    };
                    this.chatHubRepository.AddRoomBlacklistUser(room_blacklistuser);

                    var targetBlacklistUserClientModel = this.chatHubService.CreateChatHubBlacklistUserClientModel(blacklistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("AddBlacklistUser", targetBlacklistUserClientModel, roomId);
                    await this.SendClientNotification($"{targetUser.DisplayName} has been added to blacklist.", room.Id, Context.ConnectionId, contextUser.ClientModel(), ChatHubMessageType.System);
                }
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveBlacklistUser(int userId, int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (contextUser != null && targetUser != null)
                {
                    var room = await this.chatHubRepository.GetRoomById(roomId);
                    var moderators = await this.chatHubRepository.GetModerators(room).ToListAsync();
                    if (contextUser.UserId != room.CreatorId && !moderators.Any(item => item.ChatHubUserId == contextUser.UserId) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators, moderators and administrators can remove blacklis users.");
                    }

                    var blacklistUser = this.chatHubRepository.GetBlacklistUser(targetUser.UserId);
                    this.chatHubRepository.DeleteRoomBlacklistUser(roomId, blacklistUser.Id);

                    var targetBlacklistUserClientModel = this.chatHubService.CreateChatHubBlacklistUserClientModel(blacklistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveBlacklistUser", targetBlacklistUserClientModel, roomId);
                    await this.SendClientNotification($"{targetUser.DisplayName} has been removed from blacklist.", room.Id, Context.ConnectionId, contextUser.ClientModel(), ChatHubMessageType.System);
                }
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveInvitation(int invitationId)
        {
            await this.chatHubRepository.DeleteInvitation(invitationId);
        }

        private async Task AddWaitingRoomItem(ChatHubUser user, ChatHubRoom room)
        {
            var chatHubWaitingRoomItem = new ChatHubWaitingRoomItem()
            {
                Guid = Guid.NewGuid(),
                RoomId = room.Id,
                UserId = user.UserId,
                DisplayName = user.DisplayName
            };

            var roomCreator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);
            var roomCreatorConnections = await this.chatHubRepository.GetConnectionsByUserId(roomCreator.UserId).Active().ToListAsync();

            foreach (var connection in roomCreatorConnections)
            {
                await Clients.Client(connection.ConnectionId).SendAsync("AddWaitingRoomItem", chatHubWaitingRoomItem);
            }
        }
        [AllowAnonymous]
        public async Task RemoveWaitingRoomItem(ChatHubWaitingRoomItem waitingRoomItem)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var targetUser = await this.chatHubRepository.GetUserByIdAsync(waitingRoomItem.UserId);

            var targetUserConnections = await this.chatHubRepository.GetConnectionsByUserId(targetUser.UserId).Active().ToListAsync();
            await Clients.Clients(targetUserConnections.Select(item => item.ConnectionId)).SendAsync("RemovedWaitingRoomItem", waitingRoomItem);            
        }

        [AllowAnonymous]
        public async Task<ChatHubRoom> GetRoom(int roomId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var contextRoom = await this.chatHubRepository.GetRoomById(roomId);

                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(contextRoom);
                return chatHubRoomClientModel;
            }
            catch
            {
                throw new HubException("Failed get room.");
            }
        }
        [AllowAnonymous]
        public async Task<List<ChatHubRoom>> GetRoomsByModuleId()
        {
            try
            {
                int moduleId = Convert.ToInt32(Context.GetHttpContext().Request.Headers["moduleid"]);
                var contextUser = await this.GetChatHubUserAsync();

                List<ChatHubRoom> chatHubRooms = new List<ChatHubRoom>();
                List<ChatHubRoom> rooms = new List<ChatHubRoom>();
                rooms.AddRange(this.chatHubRepository.Rooms().FilterByModuleId(moduleId).Public().ToList());
                rooms.AddRange(this.chatHubRepository.Rooms().FilterByModuleId(moduleId).Private().ToList());

                if (Context.User.Identity.IsAuthenticated)
                {
                    rooms.AddRange(this.chatHubRepository.Rooms().FilterByModuleId(moduleId).Protected().ToList());
                }

                if (rooms != null && rooms.Any())
                {
                    foreach (var room in rooms)
                    {
                        var item = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);
                        chatHubRooms.Add(item);
                    }
                }

                return chatHubRooms;
            }
            catch
            {
                throw new HubException("Failed get rooms by module.");
            }
        }
        [AllowAnonymous]
        public async Task<List<ChatHubRoom>> GetLobbiesByModuleId()
        {
            try
            {
                int moduleId = Convert.ToInt32(Context.GetHttpContext().Request.Headers["moduleid"]);
                List<ChatHubRoom> lobbies = new List<ChatHubRoom>();
                List<ChatHubRoom> rooms = new List<ChatHubRoom>();
                rooms = this.chatHubRepository.Rooms().FilterByModuleId(moduleId).Where(item => item.Type != ChatHubRoomType.OneVsOne.ToString()).ToList();

                foreach (var room in rooms)
                {
                    var item = await this.chatHubService.CreateChatHubLobbyClientModel(room);
                    lobbies.Add(item);
                }

                lobbies = lobbies.OrderByDescending(item => item.Users.Count()).ThenBy(item => (int)Enum.Parse(typeof(ChatHubRoomStatus), item.Status)).ToList();
                return lobbies;
            }
            catch
            {
                throw new HubException("Failed get lobbies by module id.");
            }
        }
        [AllowAnonymous]
        public async Task<ChatHubRoom> CreateRoom(ChatHubRoom room)
        {
            var contextUser = await this.GetChatHubUserAsync();

            try
            {
                if (room.CreatorId == contextUser.UserId)
                {
                    var createdRoom = await chatHubRepository.AddRoom(room);
                    return await this.chatHubService.CreateChatHubRoomClientModelAsync(createdRoom);
                }

                return null;
            }
            catch
            {
                throw new HubException("Failed to create room.");
            }
        }
        [AllowAnonymous]
        public async Task<ChatHubRoom> UpdateRoom(ChatHubRoom room)
        {
            var contextUser = await this.GetChatHubUserAsync();

            try
            {
                if (room.CreatorId == contextUser.UserId)
                {
                    var updatedRoom = await chatHubRepository.UpdateRoomStatus(room);
                    return await this.chatHubService.CreateChatHubRoomClientModelAsync(updatedRoom);
                }

                return null;
            }
            catch
            {
                throw new HubException("Failed to update room.");
            }
        }
        [AllowAnonymous]
        public async Task DeleteRoom(int roomId)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await chatHubRepository .GetRoomById(roomId);

            try
            {
                if (contextRoom.CreatorId == contextUser.UserId || Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                {
                    this.chatHubRepository.DeleteRoomUser(roomId);
                    this.chatHubRepository.DeleteRoom(contextRoom.Id);
                }
            }
            catch
            {
                throw new HubException("Failed to delete room.");
            }
        }
        [AllowAnonymous]
        public async Task DeleteCam(int camId)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();

                var cam = await this.chatHubRepository.GetCamById(camId);
                cam.Status = ChatHubCamStatus.Deleted.ToString();
                await this.chatHubRepository.UpdateCam(cam);

                /*
                var sequenceIds = await this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == camId).Select(item => item.Id).ToListAsync();
                foreach (var id in sequenceIds)
                {
                    await this.chatHubRepository.DeleteSequence(id);
                }

                this.chatHubRepository.DeleteCam(camId);
                */
            }
            catch
            {
                throw new HubException("Failed to delete cam.");
            }
        }
        [AllowAnonymous]
        public async Task CreateExampleData()
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            var contextUser = await this.GetChatHubUserAsync();
            List<string> colors = new List<string>() { "lightskyblue", "lightpink", "lightgrey", "lightgoldenrodyellow", "lightcoral", "lightblue", "lavender", "thistle" };

            try
            {
                for(var i = 1; i <= 200; i++)
                {
                    var item = new ChatHubRoom();
                    item.ModuleId = Int32.Parse(moduleId);
                    item.Title = "post" + i;
                    item.Content = "some description";
                    item.Status = ChatHubRoomStatus.Enabled.ToString();
                    item.Type = ChatHubRoomType.Public.ToString();
                    item.CreatorId = contextUser.UserId;
                    item.ImageUrl = "";
                    item.SnapshotUrl = "";
                    item.OneVsOneId = "";
                    item.BackgroundColor = colors[new Random().Next(0, colors.Count())];
                    item.CreatedBy = contextUser.Username;
                    item.CreatedOn = DateTime.Now;
                    item.ModifiedBy = contextUser.Username;
                    item.ModifiedOn = DateTime.Now;

                    await this.chatHubRepository.AddRoom(item);
                }
            }
            catch
            {
                throw new HubException("Failed to create example data.");
            }
        }
        [AllowAnonymous]
        public async Task CreateExampleUserData(int roomId)
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];

            var contextUser = await this.GetChatHubUserAsync();
            var contextRoom = await chatHubRepository .GetRoomById(roomId);

            for (var i = 1; i <= 100; i++)
            {
                try
                {
                    string guestname = "guest";

                    string username = this.CreateUsername(guestname);
                    string displayname = this.CreateDisplaynameFromUsername(username);

                    if (await this.chatHubRepository.GetUserByUserNameAsync(displayname) != null)
                    {
                        throw new HubException("Displayname already in use. Goodbye.");
                    }

                    string email = "noreply@anyways.tv";
                    //string password = "§PasswordPolicy42";

                    ChatHubUser chatHubUser = new ChatHubUser()
                    {
                        Username = Constants.ChatHubConstants.ChatHubUserPrefix + username,
                        DisplayName = displayname,
                        Email = email,
                        PhotoFileId = null,
                        LastLoginOn = DateTime.UtcNow,
                        LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                        CreatedBy = Constants.ChatHubConstants.ChatHubUserPrefix + username,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = string.Empty,
                        ModifiedOn = DateTime.MinValue,
                        DeletedBy = null,
                        DeletedOn = null,
                        IsDeleted = false,

                        FrameworkUserId = null,
                        UserType = ChatHubUserType.Guest.ToString(),
                    };
                    var createdGuest = chatHubUser = this.chatHubRepository.AddUser(chatHubUser);

                    if (createdGuest != null && createdGuest.Username != RoleNames.Host)
                    {
                        List<Role> roles = this.roles.GetRoles(chatHubUser.SiteId).Where(item => item.IsAutoAssigned).ToList();
                        foreach (Role role in roles)
                        {
                            UserRole userrole = new UserRole();
                            userrole.UserId = createdGuest.UserId;
                            userrole.RoleId = role.RoleId;
                            userrole.EffectiveDate = null;
                            userrole.ExpiryDate = null;
                            userRoles.AddUserRole(userrole);
                        }
                    }

                    ChatHubConnection ChatHubConnection = new ChatHubConnection()
                    {
                        ChatHubUserId = createdGuest.UserId,
                        ConnectionId = Context.ConnectionId,
                        IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                        UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                        Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
                    };
                    ChatHubConnection = this.chatHubRepository.AddConnection(ChatHubConnection);

                    ChatHubSettings ChatHubSetting = new ChatHubSettings()
                    {
                        UsernameColor = "#7744aa",
                        MessageColor = "#44aa77",
                        ChatHubUserId = chatHubUser.UserId
                    };
                    ChatHubSetting = this.chatHubRepository.AddSetting(ChatHubSetting);

                    for (var x = 1; x <= 42; x++)
                    {
                        var cam = new ChatHubCam()
                        {
                            ChatHubRoomId = roomId,
                            ChatHubConnectionId = ChatHubConnection.Id,
                            Status = ChatHubCamStatus.Archived.ToString(),
                            VideoUrl = string.Empty,
                            VideoUrlExtension = string.Empty,
                        };
                        cam = await this.chatHubRepository.AddCam(cam);
                    }

                    ChatHubRoomChatHubUser room_user = new ChatHubRoomChatHubUser()
                    {
                        ChatHubRoomId = contextRoom.Id,
                        ChatHubUserId = createdGuest.UserId
                    };
                    chatHubRepository.AddRoomUser(room_user);

                    ChatHubUser guest = await this.chatHubRepository.GetUserByIdAsync(createdGuest.UserId);
                    ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(contextUser);
                    await Clients.Client(Context.ConnectionId).SendAsync("AddUser", chatHubUserClientModel, contextRoom.Id.ToString());
                }
                catch (Exception exception)
                {
                    throw new HubException(exception.Message);
                }
            }            
        }
        [AllowAnonymous]
        public async Task ArchiveActiveDbItems()
        {
            await this.chatHubService.ArchiveActiveDbItems();
        }

        [AllowAnonymous]
        public async Task<ChatHubDevice> GetDefaultDevice(int roomId, ChatHubDeviceType type)
        {
            try {
                var contextUser = await this.GetChatHubUserAsync();
                var userAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString();
                var deviceItem = await this.chatHubRepository.Devices().FirstOrDefaultAsync(item => item.ChatHubUserId == contextUser.UserId && item.ChatHubRoomId == roomId && item.UserAgent == userAgent && item.Type == type.ToString());
                ChatHubDevice deviceClientModel = null;

                if (deviceItem != null)
                {
                    deviceClientModel = deviceItem.ClientModel();
                }

                return deviceClientModel;
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        [AllowAnonymous]
        public async Task SetDefaultDevice(int roomId, string defaultDeviceId, string defaultDeviceName, ChatHubDeviceType type)
        {
            try
            {
                var contextUser = await this.GetChatHubUserAsync();
                var userAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString();
                var deviceItem = await this.chatHubRepository.Devices().FirstOrDefaultAsync(item => item.ChatHubUserId == contextUser.UserId && item.ChatHubRoomId == roomId && item.UserAgent == userAgent && item.Type == type.ToString());
                if (deviceItem == null)
                {
                    deviceItem = new ChatHubDevice()
                    {
                        UserAgent = userAgent,
                        ChatHubUserId = contextUser.UserId,
                        ChatHubRoomId = roomId,
                        DefaultDeviceId = defaultDeviceId,
                        DefaultDeviceName = defaultDeviceName,
                        Type = type.ToString(),
                        CreatedBy = contextUser.Username,
                        CreatedOn = DateTime.Now,
                        ModifiedBy = contextUser.Username,
                        ModifiedOn = DateTime.Now,
                    };
                }
                else
                {
                    deviceItem.DefaultDeviceId = defaultDeviceId;
                    deviceItem.DefaultDeviceName = defaultDeviceName;
                }

                await this.chatHubRepository.AddOrUpdateDevice(deviceItem);
            }
            catch (Exception exception)
            {
                throw new HubException(exception.Message);
            }
        }
        
        [AllowAnonymous]
        public async Task<ChatHubVisitorsDisplay> GetVisitorsDisplay(int moduleId)
        {
            
            // do please bebi add module id to connection model in database to query by connections by module id
            var moduleConnections = await this.chatHubRepository.Connections().Where(item => item.CreatedOn >= (DateTime.Now.AddHours(-24))).ToListAsync();
            var display = new ChatHubVisitorsDisplay();
            display.Items = new List<List<KeyValuePair<int, ChatHubConnection>>>();

            if (moduleConnections != null && moduleConnections.Any())
            {
                var lists = moduleConnections.GroupBy(item => item.CreatedOn.Hour);
                foreach (var list in lists)
                {
                    if (list != null && list.Any())
                    {
                        var dictionnary = new List<KeyValuePair<int, ChatHubConnection>>();
                        foreach (var item in list.Select((connection, index) => new { connection = connection, index = index }))
                        {
                            dictionnary.Add(new KeyValuePair<int, ChatHubConnection>(item.connection.CreatedOn.Hour, item.connection.ClientModel()));
                        }
                        display.Items.Add(dictionnary);
                    }
                }
            }

            return display;
        }

        [AllowAnonymous]
        public async Task AddGeolocationPosition(ChatHubGeolocation position)
        {
            var contextUser = await this.GetChatHubUserAsync();
            var connection = await this.chatHubRepository.Connections().FirstOrDefaultAsync(item => item.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                position.ChatHubConnectionId = connection.ClientModel().Id;
                position.CreatedOn = DateTime.Now;
                position.CreatedBy = contextUser.Username;
                position.ModifiedOn = DateTime.Now;
                position.ModifiedBy = contextUser.Username;

                position = this.chatHubRepository.AddGeolocation(position);
            }

            foreach (var room in await this.chatHubRepository.GetRoomsByCreator(contextUser.UserId).ToListAsync())
            {
                var exceptConnectionIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
                await this.Clients.GroupExcept(room.Id.ToString(), exceptConnectionIds).SendAsync("UpdateBingMap", room.Id, connection.Id, position.ClientModel());
            }
        }

        public async Task UpdateRoomCreator(ChatHubRoom room, List<string> exceptConnectionIds)
        {
            try
            {
                var creator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);
                ChatHubUser creatorClientModel = await this.chatHubService.CreateChatHubUserClientModelWithConnections(creator);
                await Clients.GroupExcept(room.Id.ToString(), exceptConnectionIds).SendAsync("UpdateRoomCreator", room.Id, creatorClientModel);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        private string CreateUsername(string guestname)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string id = Regex.Replace(base64Guid, "[/+=]", "");
            string userName = string.Concat(guestname, "-", new Random().Next(100000, 999999), "-", id);

            return userName;
        }
        private string CreateDisplaynameFromUsername(string username)
        {
            var name = username.Substring(0, username.IndexOf('-'));
            var numbers = username.Substring(username.IndexOf('-') + 1, 6);
            var displayname = string.Concat(name, "-", numbers);
            return displayname;
        }
        private bool IsValidGuestUsername(string guestName)
        {
            string guestNamePattern = "^([a-zA-Z0-9_]{3,32})$";
            Regex regex = new Regex(guestNamePattern);
            Match match = regex.Match(guestName);
            return match.Success;
        }
        private string GetMessageType()
        {
            return Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host) ? ChatHubMessageType.Admin.ToString()
                 : Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) ? ChatHubMessageType.Admin.ToString()
                 : Context.User.HasClaim(ClaimTypes.Role, RoleNames.Registered) ? ChatHubMessageType.User.ToString()
                 : !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Registered) ? ChatHubMessageType.Guest.ToString()
                 : ChatHubMessageType.System.ToString();
        }

    }
}