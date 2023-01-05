using Microsoft.EntityFrameworkCore;
using Oqtane.ChatHubs.Repository;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using BlazorPager;
using Microsoft.AspNetCore.Http;

namespace Oqtane.ChatHubs.Services
{
    public class ChatHubService : IService
    {

        private IHttpContextAccessor httpContextAccessor { get; set; }
        private IMemoryCache cache { get; set; }
        private ChatHubRepository chatHubRepository { get; set; }        

        private readonly string key_lobbies_prefix = "lobbies_";
        private readonly string key_users_prefix = "users_";
        private readonly string key_videos_prefix = "videos_";

        public ChatHubService(IHttpContextAccessor httpContextAccessor, ChatHubRepository chatHubRepository, IMemoryCache memoryCache)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.chatHubRepository = chatHubRepository;
            this.cache = memoryCache;
        }

        public async Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room)
        {

            IList<ChatHubCam> camsList = await this.chatHubRepository.GetCamsByRoomId(room.Id).NotArchived().ToListAsync();
            IList<ChatHubCam> camsClientModels = camsList.Select(item => this.CreateChatHubCamClientModel(item)).ToList();

            var useragent = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            IList<ChatHubDevice> deviceList = this.chatHubRepository.Devices().Where(item => item.ChatHubRoomId == room.Id && item.UserAgent == useragent).ToList();
            IList<ChatHubDevice> deviceClientModels = deviceList.Select(item => item.ClientModel()).ToList();

            ChatHubUser creator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);
            ChatHubUser creatorClientModel = await this.CreateChatHubUserClientModelWithConnections(creator);

            return new ChatHubRoom()
            {
                Id = room.Id,
                ModuleId = room.ModuleId,
                Title = room.Title,
                Content = room.Content,
                BackgroundColor = room.BackgroundColor ?? "#999999",
                ImageUrl = room.ImageUrl,
                SnapshotUrl = room.SnapshotUrl,
                Type = room.Type,
                Status = room.Status,
                OneVsOneId = room.OneVsOneId,
                CreatorId = room.CreatorId,
                Creator = creatorClientModel,
                Cams = camsClientModels,
                Devices = deviceClientModels,
                CreatedOn = room.CreatedOn,
                CreatedBy = room.CreatedBy,
                ModifiedBy = room.ModifiedBy,
                ModifiedOn = room.ModifiedOn,
                Viewers = new List<ChatHubViewer>(),
                Messages = new List<ChatHubMessage>(),
                Users = new List<ChatHubUser>(),
                Moderators = new List<ChatHubModerator>(),
                BlacklistUsers = new List<ChatHubBlacklistUser>(),
                WhitelistUsers = new List<ChatHubWhitelistUser>(),
            };
        }
        public async Task<ChatHubRoom> CreateChatHubLobbyClientModel(ChatHubRoom room)
        {
            IQueryable<ChatHubUser> onlineUsers = this.chatHubRepository.GetUsersByRoomId(room.Id).Online();
            int onlineUsersLength = onlineUsers != null && onlineUsers.Any() ? onlineUsers.Count() : 0;

            var creator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);
            var creatorConnections = await this.chatHubRepository.GetConnectionsByUserId(creator.UserId).Active().ToListAsync();
            var creatorClientModel = this.CreateChatHubUserClientModel(creator);

            IQueryable<ChatHubCam> camsQuery = this.chatHubRepository.GetCamsByRoomId(room.Id).Broadcasting();
            bool broadcasting = camsQuery.ToList().Any(cam => creatorConnections.Any(connection => connection.Id == cam.ChatHubConnectionId));

            return new ChatHubRoom()
            {
                Id = room.Id,
                ModuleId = room.ModuleId,
                Title = room.Title,
                Content = room.Content,
                BackgroundColor = room.BackgroundColor ?? "#999999",
                ImageUrl = room.ImageUrl,
                SnapshotUrl = room.SnapshotUrl,
                Type = room.Type,
                Status = room.Status,
                OneVsOneId = room.OneVsOneId,
                CreatorId = room.CreatorId,
                Creator = creatorClientModel,
                UsersLength = onlineUsersLength,
                ViewersLength = 0,
                Broadcasting = broadcasting,
                CreatedOn = room.CreatedOn,
                CreatedBy = room.CreatedBy,
                ModifiedBy = room.ModifiedBy,
                ModifiedOn = room.ModifiedOn
            };
        }
        public ChatHubUser CreateChatHubUserClientModel(ChatHubUser user)
        {
            ChatHubSettings chatHubSettings = this.chatHubRepository.GetSetting(user.UserId);
            ChatHubSettings chatHubSettingsClientModel = chatHubSettings != null ? this.CreateChatHubSettingClientModel(chatHubSettings) : null;

            return new ChatHubUser()
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                UserlistItemCollapsed = user.UserlistItemCollapsed,
                Settings = chatHubSettingsClientModel,
                Connections = new List<ChatHubConnection>(),
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy
            };
        }
        public async Task<ChatHubUser> CreateChatHubUserClientModelWithConnections(ChatHubUser user)
        {
            List<ChatHubConnection> activeConnections = await this.chatHubRepository.GetConnectionsByUserId(user.UserId).Active().ToListAsync();
            List<ChatHubConnection> activeConnectionsClientModels = new List<ChatHubConnection>();
            foreach (var activeConnection in activeConnections)
            {
                activeConnectionsClientModels.Add(await CreateChatHubConnectionClientModel(activeConnection));
            }

            ChatHubSettings chatHubSettings = this.chatHubRepository.GetSetting(user.UserId);
            ChatHubSettings chatHubSettingsClientModel = chatHubSettings != null ? this.CreateChatHubSettingClientModel(chatHubSettings) : null;

            return new ChatHubUser()
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Settings = chatHubSettingsClientModel,
                Connections = activeConnectionsClientModels,
                UserlistItemCollapsed = user.UserlistItemCollapsed,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy,
            };
        }
        public ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message, ChatHubUser user, List<ChatHubPhoto> pics = null)
        {
            List<ChatHubPhoto> photosClientModels = pics != null && pics.Any() ? pics.Select(item => CreateChatHubPhotoClientModel(item)).ToList() : null;
            ChatHubUser userClientModel = this.CreateChatHubUserClientModel(user);

            return new ChatHubMessage()
            {
                Id = message.Id,
                ChatHubRoomId = message.ChatHubRoomId,
                ChatHubUserId = message.ChatHubUserId,
                Content = message.Content,
                Type = message.Type,
                User = userClientModel,
                Photos = photosClientModels,
                CommandMetaDatas = message.CommandMetaDatas,
                CreatedOn = message.CreatedOn,
                CreatedBy = message.CreatedBy,
                ModifiedOn = message.ModifiedOn,
                ModifiedBy = message.ModifiedBy
            };
        }
        public async Task<ChatHubConnection> CreateChatHubConnectionClientModel(ChatHubConnection connection)
        {
            var camItems = await this.chatHubRepository.GetCamsByConnectionId(connection.Id).NotArchived().ToListAsync();
            var camClientModels = camItems != null ? camItems.Select(cam => this.CreateChatHubCamClientModel(cam)).ToList() : null;

            return new ChatHubConnection()
            {
                Id = connection.Id,
                ChatHubUserId = connection.ChatHubUserId,
                ConnectionId = connection.ConnectionId,
                IpAddress = connection.IpAddress,
                UserAgent = connection.UserAgent,
                Status = connection.Status,
                User = connection.User,
                Cams = camClientModels,
                CreatedOn = connection.CreatedOn,
                CreatedBy = connection.CreatedBy,
                ModifiedOn = connection.ModifiedOn,
                ModifiedBy = connection.ModifiedBy
            };
        }
        public ChatHubPhoto CreateChatHubPhotoClientModel(ChatHubPhoto photo)
        {
            return new ChatHubPhoto()
            {
                Id = photo.Id,
                ChatHubMessageId = photo.ChatHubMessageId,
                Source = photo.Source,
                Thumb = photo.Thumb,
                Caption = photo.Caption,
                Size = photo.Size,
                Width = photo.Width,
                Height = photo.Height,
                CreatedOn = photo.CreatedOn,
                CreatedBy = photo.CreatedBy,
                ModifiedOn = photo.ModifiedOn,
                ModifiedBy = photo.ModifiedBy
            };
        }
        public ChatHubCam CreateChatHubCamClientModel(ChatHubCam cam)
        {
            return new ChatHubCam()
            {
                Id = cam.Id,
                ChatHubConnectionId = cam.ChatHubConnectionId,
                ChatHubRoomId = cam.ChatHubRoomId,
                Status = cam.Status,
                TotalVideoSequences = cam.TotalVideoSequences,
                VideoUrl = cam.VideoUrl,
                VideoUrlExtension = cam.VideoUrlExtension,
                CreatedOn = cam.CreatedOn,
                CreatedBy = cam.CreatedBy,
                ModifiedOn = cam.ModifiedOn,
                ModifiedBy = cam.ModifiedBy
            };
        }
        public ChatHubSettings CreateChatHubSettingClientModel(ChatHubSettings settings)
        {
            return new ChatHubSettings()
            {
                UsernameColor = settings.UsernameColor,
                MessageColor = settings.MessageColor,
                CreatedOn = settings.CreatedOn,
                CreatedBy = settings.CreatedBy,
                ModifiedOn = settings.ModifiedOn,
                ModifiedBy = settings.ModifiedBy
            };
        }
        public ChatHubModerator CreateChatHubModeratorClientModel(ChatHubModerator moderator)
        {
            return new ChatHubModerator()
            {
                Id = moderator.Id,
                ModeratorDisplayName = moderator.ModeratorDisplayName,
                ChatHubUserId = moderator.ChatHubUserId,
            };
        }
        public ChatHubWhitelistUser CreateChatHubWhitelistUserClientModel(ChatHubWhitelistUser whitelistUser)
        {
            return new ChatHubWhitelistUser()
            {
                Id = whitelistUser.Id,
                WhitelistUserDisplayName = whitelistUser.WhitelistUserDisplayName,
                ChatHubUserId = whitelistUser.ChatHubUserId,
            };
        }
        public ChatHubBlacklistUser CreateChatHubBlacklistUserClientModel(ChatHubBlacklistUser blacklistUser)
        {
            return new ChatHubBlacklistUser()
            {
                Id = blacklistUser.Id,
                BlacklistUserDisplayName = blacklistUser.BlacklistUserDisplayName,
                ChatHubUserId = blacklistUser.ChatHubUserId,
            };
        }

        public async Task<BlazorPagerApiItem<ChatHubRoom>> GetRooms(int page, int items, int moduleId, bool renewCache)
        {
            try
            {
                var pageKey = string.Concat(this.key_lobbies_prefix,
                                        moduleId, "_",
                                        page);

                BlazorPagerApiItem<ChatHubRoom> cachedPagerApiItem = new BlazorPagerApiItem<ChatHubRoom>();
                if (renewCache || !this.cache.TryGetValue<BlazorPagerApiItem<ChatHubRoom>>(pageKey, out cachedPagerApiItem))
                {
                    List<ChatHubRoom> lobbies = new List<ChatHubRoom>();
                    List<ChatHubRoom> rooms = new List<ChatHubRoom>();
                    rooms = this.chatHubRepository.Rooms()
                                                  .FilterByModuleId(moduleId)
                                                  .Where(item => item.Type != ChatHubRoomType.OneVsOne.ToString())
                                                  .ToList();

                    foreach (var room in rooms)
                    {
                        var item = await this.CreateChatHubLobbyClientModel(room);
                        lobbies.Add(item);
                    }

                    lobbies = lobbies
                        .OrderByDescending(item => item.Broadcasting)
                        .ThenByDescending(item => item.UsersLength)
                        .ThenBy(item => (int)Enum.Parse(typeof(ChatHubRoomStatus), item.Status))
                        .Skip((page - 1) * items)
                        .Take(items)
                        .ToList();

                    foreach (var lobby in lobbies)
                    {
                        IList<ChatHubViewer> viewers = await this.chatHubRepository.GetViewersByRoomIdAsync(lobby.Id);
                        lobby.ViewersLength = viewers.Count();
                    }

                    var itemsTotal = await this.chatHubRepository.Rooms().FilterByModuleId(moduleId).Where(item => item.Type != ChatHubRoomType.OneVsOne.ToString()).CountAsync();
                    int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                    BlazorPagerApiItem<ChatHubRoom> apiItem = new BlazorPagerApiItem<ChatHubRoom>()
                    {
                        Items = lobbies,
                        Pages = pagesTotal,
                    };

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                    cachedPagerApiItem = this.cache.Set<BlazorPagerApiItem<ChatHubRoom>>(pageKey, apiItem, cacheEntryOptions);
                }

                return cachedPagerApiItem;
            }
            catch
            {
                throw new Exception("Failed get lobbies by module id.");
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubUser>> GetUsers(int page, int items, int roomId, bool renewCache)
        {
            try
            {
                var pageKey = string.Concat(this.key_users_prefix,
                                        roomId, "_",
                                        page);

                BlazorPagerApiItem<ChatHubUser> cachedPagerApiItem = new BlazorPagerApiItem<ChatHubUser>();
                if (renewCache || !this.cache.TryGetValue<BlazorPagerApiItem<ChatHubUser>>(pageKey, out cachedPagerApiItem))
                {
                    List<ChatHubUser> userClientModels = new List<ChatHubUser>();
                    List<ChatHubUser> dbUsers = new List<ChatHubUser>();
                    var dbQuery = this.chatHubRepository.GetUsersByRoomId(roomId).Online();

                    List<ChatHubUser> orderableList = new List<ChatHubUser>();
                    orderableList = dbQuery.Select(item => new ChatHubUser()
                        {
                            UserId = item.UserId,
                            Username = item.Username,
                            DisplayName = item.DisplayName,
                            UserlistItemCollapsed = item.UserlistItemCollapsed,
                            CreatedOn = item.CreatedOn,
                            CreatedBy = item.CreatedBy,
                            ModifiedOn = item.ModifiedOn,
                            ModifiedBy = item.ModifiedBy,
                        }).ToList();

                    var orderedList = orderableList
                        .OrderBy(item => item.DisplayName)
                        .Skip((page - 1) * items)
                        .Take(items)
                        .ToList();

                    foreach (var user in orderedList)
                    {
                        var item = this.CreateChatHubUserClientModel(user);
                        userClientModels.Add(item);
                    }

                    var itemsTotal = await this.chatHubRepository.GetUsersByRoomId(roomId).Online().CountAsync();
                    int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                    BlazorPagerApiItem<ChatHubUser> apiItem = new BlazorPagerApiItem<ChatHubUser>()
                    {
                        Items = userClientModels,
                        Pages = pagesTotal,
                    };

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                    cachedPagerApiItem = this.cache.Set<BlazorPagerApiItem<ChatHubUser>>(pageKey, apiItem, cacheEntryOptions);
                }

                return cachedPagerApiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubCam>> GetArchiveItems(int page, int items, int moduleId, bool renewCache, ChatHubUser user)
        {
            try
            {
                var pageKey = string.Concat(this.key_videos_prefix,
                                        moduleId, "_",
                                        page);

                BlazorPagerApiItem<ChatHubCam> cachedPagerApiItem = new BlazorPagerApiItem<ChatHubCam>();
                if (renewCache || !this.cache.TryGetValue<BlazorPagerApiItem<ChatHubCam>>(pageKey, out cachedPagerApiItem))
                {
                    List<ChatHubCam> videos = new List<ChatHubCam>();
                    var connections = await this.chatHubRepository.GetConnectionsByUserId(user.UserId).ToListAsync();

                    foreach (var connection in connections)
                    {
                        var cams = await this.chatHubRepository.GetCamsByConnectionId(connection.Id).Where(item => item.Sequences.Any()).Where(item => item.Status != ChatHubCamStatus.Deleted.ToString()).Skip(page -1).Take(items).ToListAsync();
                        foreach (var cam in cams)
                        {
                            cam.TotalVideoSequences = await this.chatHubRepository.CamSequences().Where(item => item.ChatHubCamId == cam.Id).CountAsync();
                            videos.Add(cam);
                        }
                    }

                    var itemsTotal = videos.Count();
                    int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                    BlazorPagerApiItem<ChatHubCam> apiItem = new BlazorPagerApiItem<ChatHubCam>()
                    {
                        Items = videos,
                        Pages = pagesTotal,
                    };

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                    cachedPagerApiItem = this.cache.Set<BlazorPagerApiItem<ChatHubCam>>(pageKey, apiItem, cacheEntryOptions);
                }

                return cachedPagerApiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubInvitation>> GetInvitationItems(int page, int items, int moduleId, ChatHubUser user)
        {
            try
            {
                BlazorPagerApiItem<ChatHubInvitation> apiItem = null;
                List<ChatHubInvitation> invitations = new List<ChatHubInvitation>();

                var dbQuery = this.chatHubRepository.Invitations().Where(item => item.ChatHubUserId == user.UserId);
                List<ChatHubInvitation> orderableList = new List<ChatHubInvitation>();
                orderableList = await dbQuery.Select(item => new ChatHubInvitation()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    RoomId = item.RoomId,
                    Hostname = item.Hostname,
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                invitations = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = invitations.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubInvitation>()
                {
                    Items = invitations ?? new List<ChatHubInvitation>(),
                    Pages = pagesTotal,
                };                    
                
                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubIgnore>> GetIgnoreItems(int page, int items, int moduleId, ChatHubUser user)
        {
            try
            {
                BlazorPagerApiItem<ChatHubIgnore> apiItem = null;
                List<ChatHubIgnore> ignores = new List<ChatHubIgnore>();

                var dbQuery = this.chatHubRepository.Ignores().Include(item => item.User).Where(item => item.ChatHubUserId == user.UserId);
                List<ChatHubIgnore> orderableList = new List<ChatHubIgnore>();
                orderableList = await dbQuery.Select(item => new ChatHubIgnore()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    ChatHubIgnoredUserId = item.ChatHubIgnoredUserId,
                    User = this.chatHubRepository.Users().FirstOrDefault(u => u.UserId == item.ChatHubIgnoredUserId).ClientModel(),
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                ignores = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = ignores.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubIgnore>()
                {
                    Items = ignores ?? new List<ChatHubIgnore>(),
                    Pages = pagesTotal,
                };

                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubIgnoredBy>> GetIgnoredByItems(int page, int items, int moduleId, ChatHubUser user)
        {
            try
            {
                BlazorPagerApiItem<ChatHubIgnoredBy> apiItem = null;
                List<ChatHubIgnoredBy> ignores = new List<ChatHubIgnoredBy>();

                var dbQuery = this.chatHubRepository.Ignores().Include(item => item.User).Where(item => item.ChatHubIgnoredUserId == user.UserId);
                List<ChatHubIgnoredBy> orderableList = new List<ChatHubIgnoredBy>();
                orderableList = await dbQuery.Select(item => new ChatHubIgnoredBy()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    ChatHubIgnoredUserId = item.ChatHubIgnoredUserId,
                    User = this.chatHubRepository.Users().FirstOrDefault(u => u.UserId == item.ChatHubUserId).ClientModel(),
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                ignores = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = ignores.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubIgnoredBy>()
                {
                    Items = ignores ?? new List<ChatHubIgnoredBy>(),
                    Pages = pagesTotal,
                };

                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubModerator>> GetModeratorItems(int page, int items, int roomId, ChatHubUser user)
        {
            try
            {
                ChatHubRoom room = await this.chatHubRepository.GetRoomById(roomId);
                BlazorPagerApiItem<ChatHubModerator> apiItem = null;
                List<ChatHubModerator> moderators = new List<ChatHubModerator>();

                var dbQuery = this.chatHubRepository.GetModerators(room);
                List<ChatHubModerator> orderableList = new List<ChatHubModerator>();
                orderableList = await dbQuery.Select(item => new ChatHubModerator()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    ModeratorDisplayName = item.ModeratorDisplayName,
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                moderators = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = moderators.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubModerator>()
                {
                    Items = moderators ?? new List<ChatHubModerator>(),
                    Pages = pagesTotal,
                };

                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubBlacklistUser>> GetBlacklistUserItems(int page, int items, int roomId, ChatHubUser user)
        {
            try
            {
                ChatHubRoom room = await this.chatHubRepository.GetRoomById(roomId);
                BlazorPagerApiItem<ChatHubBlacklistUser> apiItem = null;
                List<ChatHubBlacklistUser> blacklistUsers = new List<ChatHubBlacklistUser>();

                var dbQuery = this.chatHubRepository.GetBlacklistUsers(room);
                List<ChatHubBlacklistUser> orderableList = new List<ChatHubBlacklistUser>();
                orderableList = await dbQuery.Select(item => new ChatHubBlacklistUser()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    BlacklistUserDisplayName = item.BlacklistUserDisplayName,
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                blacklistUsers = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = blacklistUsers.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubBlacklistUser>()
                {
                    Items = blacklistUsers ?? new List<ChatHubBlacklistUser>(),
                    Pages = pagesTotal,
                };

                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        public async Task<BlazorPagerApiItem<ChatHubWhitelistUser>> GetWhitelistUserItems(int page, int items, int roomId, ChatHubUser user)
        {
            try
            {
                ChatHubRoom room = await this.chatHubRepository.GetRoomById(roomId);
                BlazorPagerApiItem<ChatHubWhitelistUser> apiItem = null;
                List<ChatHubWhitelistUser> moderators = new List<ChatHubWhitelistUser>();

                var dbQuery = this.chatHubRepository.GetWhitelistUsers(room);
                List<ChatHubWhitelistUser> orderableList = new List<ChatHubWhitelistUser>();
                orderableList = await dbQuery.Select(item => new ChatHubWhitelistUser()
                {
                    Id = item.Id,
                    ChatHubUserId = item.ChatHubUserId,
                    WhitelistUserDisplayName = item.WhitelistUserDisplayName,
                    CreatedOn = item.CreatedOn,
                    CreatedBy = item.CreatedBy,
                    ModifiedOn = item.ModifiedOn,
                    ModifiedBy = item.ModifiedBy,
                }).ToListAsync();

                moderators = orderableList
                    .OrderByDescending(item => item.CreatedOn)
                    .Skip((page - 1) * items)
                    .Take(items)
                    .ToList();

                var itemsTotal = moderators.Count();
                int pagesTotal = Convert.ToInt32(Math.Ceiling(itemsTotal / Convert.ToDouble(items)));

                apiItem = new BlazorPagerApiItem<ChatHubWhitelistUser>()
                {
                    Items = moderators ?? new List<ChatHubWhitelistUser>(),
                    Pages = pagesTotal,
                };

                return apiItem;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task IgnoreUser(ChatHubUser callerUser, ChatHubUser targetUser)
        {
            ChatHubIgnore chatHubIgnore = this.chatHubRepository.GetIgnore(callerUser.UserId, targetUser.UserId);
            if (chatHubIgnore != null)
            {
                chatHubIgnore.ModifiedOn = DateTime.Now;
                await this.chatHubRepository.UpdateIgnore(chatHubIgnore);
            }
            else
            {
                chatHubIgnore = new ChatHubIgnore()
                {
                    ChatHubUserId = callerUser.UserId,
                    ChatHubIgnoredUserId = targetUser.UserId,
                    CreatedBy = callerUser.Username,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = callerUser.Username,
                    ModifiedOn = DateTime.Now,
                };

                this.chatHubRepository.AddIgnore(chatHubIgnore);
            }
        }

        public List<string> GetAllExceptConnectionIds(ChatHubUser user)
        {
            var list = new List<ChatHubUser>();

            var ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();
            var ignoredByUsers = this.chatHubRepository.GetIgnoredByApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();

            list.AddRange(ignoredUsers);
            list.AddRange(ignoredByUsers);

            List<string> connectionsIds = new List<string>();

            foreach (var item in list)
            {
                foreach (var connection in item.Connections.Active())
                {
                    connectionsIds.Add(connection.ConnectionId);
                }
            }

            return connectionsIds;
        }
        public async Task<ChatHubRoom> GetOneVsOneRoomAsync(ChatHubUser callerUser, ChatHubUser targetUser, int moduleId)
        {
            if (callerUser != null && targetUser != null)
            {
                var oneVsOneRoom = this.chatHubRepository.GetRoomOneVsOne(this.CreateOneVsOneId(callerUser, targetUser));
                if(oneVsOneRoom != null)
                {
                    return oneVsOneRoom;
                }

                ChatHubRoom chatHubRoom = new ChatHubRoom()
                {
                    ModuleId = moduleId,
                    Title = string.Format("{0} vs {1}", callerUser.DisplayName, targetUser.DisplayName),
                    Content = "One Vs One",
                    BackgroundColor = "#999999",
                    Type = ChatHubRoomType.OneVsOne.ToString(),
                    Status = ChatHubRoomStatus.Enabled.ToString(),
                    ImageUrl = string.Empty,
                    SnapshotUrl = string.Empty,
                    OneVsOneId = this.CreateOneVsOneId(callerUser, targetUser),
                    CreatorId = callerUser.UserId
                };
                return await this.chatHubRepository.AddRoom(chatHubRoom);
            }

            return null;
        }
        public string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2)
        {
            var list = new List<string>();
            list.Add(user1.UserId.ToString());
            list.Add(user2.UserId.ToString());
            list = list.OrderBy(item => item).ToList();
            string roomId = string.Concat(list.First(), "|", list.Last());

            return roomId;
        }
        public bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller)
        {
            return room.OneVsOneId.Split('|').OrderBy(item => item).Any(item => item == caller.UserId.ToString());
        }
        public bool IsWhitelisted(ChatHubRoom room, ChatHubUser caller)
        {
            var whitelistuser = this.chatHubRepository.GetWhitelistUser(caller.UserId);
            if(whitelistuser != null)
            {
                var room_whitelistuser = this.chatHubRepository.GetRoomWhitelistUser(room.Id, whitelistuser.Id);

                if (room_whitelistuser != null || caller.UserId == room.CreatorId)
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsBlacklisted(ChatHubRoom room, ChatHubUser caller)
        {
            var blacklistuser = this.chatHubRepository.GetBlacklistUser(caller.UserId);
            if(blacklistuser != null)
            {
                var room_blacklistuser = this.chatHubRepository.GetRoomBlacklistUser(room.Id, blacklistuser.Id);

                if (room_blacklistuser != null)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task ArchiveActiveDbItems()
        {
            try
            {
                var activeConnections = await chatHubRepository.Connections().Active().ToListAsync();
                foreach (var connection in activeConnections)
                {
                    connection.Status = ChatHubConnectionStatus.Archived.ToString();
                    await chatHubRepository.UpdateConnection(connection);
                }

                var cams = await this.chatHubRepository.Cams().NotArchived().ToListAsync();
                foreach (var cam in cams.Where(item => item.Status != ChatHubCamStatus.Archived.ToString() && item.Status != ChatHubCamStatus.Deleted.ToString()))
                {
                    cam.Status = ChatHubCamStatus.Archived.ToString();
                    await this.chatHubRepository.UpdateCam(cam);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

    }
}
