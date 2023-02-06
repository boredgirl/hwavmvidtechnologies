using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.Modules;
using Oqtane.ChatHubs.Models;
using Oqtane.ChatHubs.Extensions;
using Oqtane.ChatHubs.Enums;
using BlazorAlerts;
using BlazorDraggableList;
using Hwavmvid.Video;
using BlazorBrowserResize;
using Hwavmvid.Notifications;
using Hwavmvid.VideoPlayer;
using Hwavmvid.Devices;
using System.Text.Json;
using Hwavmvid.Jsapigeolocation;

namespace Oqtane.ChatHubs.Services
{

    public class ChatHubService : ServiceBase, IService, IDisposable
    {

        public IJSRuntime JSRuntime { get; set; }
        public HttpClient HttpClient { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public SiteState SiteState { get; set; }        
        public ScrollService ScrollService { get; set; }
        public BlazorAlertsService BlazorAlertsService { get; set; }
        public BlazorDraggableListService BlazorDraggableListService { get; set; }
        public BlazorBrowserResizeService BrowserResizeService { get; set; }
        public VideoService VideoService { get; set; }
        public VideoPlayerService VideoPlayerService { get; set; }
        public NotificationsService NotificationsService { get; set; }
        public DevicesService DevicesService { get; set; }
        public Jsapigeolocationservice Jsapigeolocationservice { get; set; }
        public Jsapibingmapservice Jsapibingmapservice { get; set; }

        public int ModuleId { get; set; }
        public string ContextRoomId { get; set; }
        public HubConnection Connection { get; set; }
        public ChatHubUser ConnectedUser { get; set; }
        public Cookie IdentityCookie { get; set; }
        public List<ChatHubRoom> Lobbies { get; set; } = new List<ChatHubRoom>();
        public List<ChatHubRoom> Rooms { get; set; } = new List<ChatHubRoom>();
        public List<ChatHubInvitation> Invitations { get; set; } = new List<ChatHubInvitation>();
        public List<ChatHubIgnore> IgnoredUsers { get; set; } = new List<ChatHubIgnore>();
        public List<ChatHubIgnoredBy> IgnoredByUsers { get; set; } = new List<ChatHubIgnoredBy>();
        public List<ChatHubCam> VideoCaptures { get; set; } = new List<ChatHubCam>();

        public Timer GetViewersTimer { get; set; } = new Timer();

        public event EventHandler OnUpdateUI;
        public event EventHandler<ChatHubUser> OnUpdateConnectedUserEvent;
        public event EventHandler<ChatHubRoom> OnAddChatHubRoomEvent;
        public event EventHandler<ChatHubRoom> OnRemoveChatHubRoomEvent;
        public event Action<ChatHubUser, string> OnAddChatHubUserEvent;
        public event Action<ChatHubUser, string> OnRemoveChatHubUserEvent;
        public event EventHandler<ChatHubMessage> OnAddChatHubMessageEvent;
        public event EventHandler<ChatHubWaitingRoomItem> OnAddChatHubWaitingRoomItemEvent;
        public event EventHandler<ChatHubWaitingRoomItem> OnRemovedChatHubWaitingRoomItemEvent;
        public event Action<ChatHubCam, int> OnAddChatHubCamEvent;
        public event Action<ChatHubCam, int> OnRemoveChatHubCamEvent;
        public event Action<string, string, string> OnDownloadBytesEvent;
        public event Action<int, ChatHubUser> OnUpdateRoomCreatorEvent;
        public event Action<int, int, ChatHubGeolocation> Onupdatebingmapevent;
        public event EventHandler<int> OnClearHistoryEvent;
        public event EventHandler<string> OnMatchedEvent;

        public IJSObjectReference chatHubScriptJsObjRef { get; set; }
        public IJSObjectReference chatHubMap { get; set; }

        public bool Connected 
        { 
            get
            {
                return this.Connection != null && this.Connection.State == HubConnectionState.Connected;
            }
        }

        public ChatHubService(HttpClient httpClient, SiteState siteState, NavigationManager navigationManager, IJSRuntime JSRuntime, ScrollService scrollService, BlazorAlertsService blazorAlertsService, BlazorDraggableListService blazorDraggableListService, BlazorBrowserResizeService browserResizeService, VideoService VideoService, VideoPlayerService VideoPlayerService, NotificationsService Notificationservice, DevicesService DevicesService, Jsapigeolocationservice jsapigeolocationservice, Jsapibingmapservice jsapibingmapservice) : base (httpClient)
        {
            this.HttpClient = httpClient;
            this.SiteState = siteState;
            this.NavigationManager = navigationManager;
            this.JSRuntime = JSRuntime;
            this.ScrollService = scrollService;
            this.BlazorAlertsService = blazorAlertsService;
            this.BlazorDraggableListService = blazorDraggableListService;
            this.BrowserResizeService = browserResizeService;
            this.VideoService = VideoService;
            this.VideoPlayerService = VideoPlayerService;
            this.NotificationsService = Notificationservice;
            this.DevicesService = DevicesService;
            this.Jsapigeolocationservice = jsapigeolocationservice;
            this.Jsapibingmapservice = jsapibingmapservice;

            this.VideoService.StartVideoEvent += async (VideoModel model) => await this.StartCam(model);
            this.VideoService.StopVideoEvent += async (VideoModel model) => await this.StopCam(model);
            this.VideoService.VideoServiceExtension.OnDataAvailableEventHandler += async (string data, string id1, string id2) => await OnDataAvailableEventHandlerExecute(data, id1, id2);
            this.VideoService.TookSnapshotEvent += async (string imageUri, string roomId, string camId, VideoSnapshotActivatorType snapshotActivatorType) => await this.TookSnapshotEventExecute(imageUri, roomId, camId, snapshotActivatorType);

            this.VideoPlayerService.VideoServiceExtension.OnGetNextSequence += async (VideoPlayerModel item) => await OnGetNextSequenceExecute(item);

            this.BlazorAlertsService.OnAlertConfirmed += OnAlertConfirmedExecute;

            this.OnUpdateConnectedUserEvent += OnUpdateConnectedUserExecute;
            this.OnAddChatHubRoomEvent += OnAddChatHubRoomExecute;
            this.OnRemoveChatHubRoomEvent += OnRemoveChatHubRoomExecute;
            this.OnAddChatHubUserEvent += OnAddChatHubUserExecute;
            this.OnRemoveChatHubUserEvent += OnRemoveChatHubUserExecute;
            this.OnAddChatHubMessageEvent += OnAddChatHubMessageExecute;
            this.OnAddChatHubWaitingRoomItemEvent += OnAddChatHubWaitingRoomItemExecute;
            this.OnRemovedChatHubWaitingRoomItemEvent += OnRemovedChathubWaitingRoomItemExecute;
            this.OnAddChatHubCamEvent += OnAddChatHubCamExecute;
            this.OnRemoveChatHubCamEvent += OnRemoveChatHubCamExecute;
            this.OnDownloadBytesEvent += async (string dataURI, string roomId, string camId) => await OnDownloadBytesExecuteAsync(dataURI, roomId, camId);
            this.OnUpdateRoomCreatorEvent += OnUpdateRoomCreatorExecute;
            this.Onupdatebingmapevent += async (int roomId, int connectionId, ChatHubGeolocation position) => await Onupdatebingmapexecute(roomId, connectionId, position);
            this.OnClearHistoryEvent += OnClearHistoryExecute;
            this.OnMatchedEvent += MatchedEventExecute;

            this.Jsapigeolocationservice.OnGeolocationpositionReceived += async (Jsapigeolocationpositionevent item) => await GeolocationreceivedAsync(item);

            this.DevicesService.OnAudioDeviceChanged += async (DevicesEvent e) => await OnAudioDeviceChanged(e);
            this.DevicesService.OnMicrophoneDeviceChanged += async (DevicesEvent e) => await OnMicrophoneDeviceChanged(e);
            this.DevicesService.OnWebcamDeviceChanged += async (DevicesEvent e) => await OnWebcamDeviceChanged(e);

            this.GetViewersTimer.Elapsed += new ElapsedEventHandler(async (object source, ElapsedEventArgs e) => await OnGetViewersTimerElapsed(source, e));
            this.GetViewersTimer.Interval = 10000;
            this.GetViewersTimer.Enabled = true;
        }

        private void MatchedEventExecute(object sender, string e)
        {
            this.BlazorAlertsService.NewBlazorAlert(e, "Javascript Application", PositionType.Fixed);
        }

        public async Task InitChatHubService()
        {
            this.chatHubScriptJsObjRef = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/chathubjsinterop.js");
            this.chatHubMap = await this.chatHubScriptJsObjRef.InvokeAsync<IJSObjectReference>("initchathub");
        }

        public async Task ConsoleLog(string msg)
        {
            await this.chatHubMap.InvokeVoidAsync("consolelog", msg);
        }

        public void BuildHubConnection(string username, int moduleId)
        {
            StringBuilder urlBuilder = new StringBuilder();
            var chatHubConnection = this.NavigationManager.BaseUri + "1/api/chathub";

            urlBuilder.Append(chatHubConnection);
            urlBuilder.Append("?guestname=" + username);

            var url = urlBuilder.ToString();
            Connection = new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.Cookies.Add(this.IdentityCookie);
                options.Headers.Add("moduleid", moduleId.ToString());
                options.Headers.Add("platform", "Oqtane");
                options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.WriteIndented = false;
                options.PayloadSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                options.PayloadSerializerOptions.AllowTrailingCommas = true;
                options.PayloadSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                options.PayloadSerializerOptions.DefaultBufferSize = 4096;
                options.PayloadSerializerOptions.MaxDepth = 41;
                options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            })
            .Build();
        }
        public void RegisterHubConnectionHandlers()
        {
            this.Connection.Reconnecting += (ex) =>
            {
                return Task.CompletedTask;
            };
            this.Connection.Reconnected += (msg) =>
            {
                return Task.CompletedTask;
            };
            this.Connection.Closed += (ex) =>
            {
                this.Rooms.Clear();
                this.RunUpdateUI();
                return Task.CompletedTask;
            };

            this.Connection.On("OnUpdateConnectedUser", (ChatHubUser user) => OnUpdateConnectedUserEvent(this, user));
            this.Connection.On("AddRoom", (ChatHubRoom room) => OnAddChatHubRoomEvent(this, room));
            this.Connection.On("RemoveRoom", (ChatHubRoom room) => OnRemoveChatHubRoomEvent(this, room));
            this.Connection.On("AddUser", (ChatHubUser user, string roomId) => OnAddChatHubUserEvent(user, roomId));
            this.Connection.On("RemoveUser", (ChatHubUser user, string roomId) => OnRemoveChatHubUserEvent(user, roomId));
            this.Connection.On("AddMessage", (ChatHubMessage message) => OnAddChatHubMessageEvent(this, message));
            this.Connection.On("AddWaitingRoomItem", (ChatHubWaitingRoomItem waitingRoomItem) => OnAddChatHubWaitingRoomItemEvent(this, waitingRoomItem));
            this.Connection.On("RemovedWaitingRoomItem", (ChatHubWaitingRoomItem waitingRoomItem) => OnRemovedChatHubWaitingRoomItemEvent(this, waitingRoomItem));
            this.Connection.On("DownloadBytes", (string dataURI, string id, string connectionId) => OnDownloadBytesEvent(dataURI, id, connectionId));
            this.Connection.On("UpdateRoomCreator", (int roomId, ChatHubUser user) => OnUpdateRoomCreatorEvent(roomId, user));
            this.Connection.On("UpdateBingMap", (int roomId, int connectionId, ChatHubGeolocation position) => Onupdatebingmapevent(roomId, connectionId, position));
            this.Connection.On("AddCam", (ChatHubCam cam, int roomId) => OnAddChatHubCamEvent(cam, roomId));
            this.Connection.On("RemoveCam", (ChatHubCam cam, int roomId) => OnRemoveChatHubCamEvent(cam, roomId));
            this.Connection.On("ClearHistory", (int roomId) => OnClearHistoryEvent(this, roomId));
            this.Connection.On("Matched", (string message) => OnMatchedEvent(this, message));
        }
        public async Task ConnectAsync()
        {
            await this.Connection.StartAsync().ContinueWith(async task =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;

                    await this.Connection.SendAsync("Init").ContinueWith((task) =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                        {
                            if (this.TryHandleException(task))
                                return;
                        }
                    });
                }
            });
        }
        public async Task<bool> ConnectToChat(string GuestUsername, int ModuleId)
        {
            try
            {
                if (this.Connection?.State == HubConnectionState.Connected
                 || this.Connection?.State == HubConnectionState.Connecting
                 || this.Connection?.State == HubConnectionState.Reconnecting)
                {
                    this.BlazorAlertsService.NewBlazorAlert($"The client is already connected. Trying establish new connection with guest name { GuestUsername }.", "Javascript Application", PositionType.Fixed);
                }

                this.BuildHubConnection(GuestUsername, ModuleId);
                this.RegisterHubConnectionHandlers();
                await this.ConnectAsync();
                return true;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }

            return false;
        }

        public async Task OnDataAvailableEventHandlerExecute(string dataUri, string id1, string id2)
        {
            try
            {
                if (this.Connection?.State == HubConnectionState.Connected)
                {
                    var room = this.Rooms.FirstOrDefault(item => item.Id == Convert.ToInt32(id1));
                    if (room != null)
                    {
                        await this.Connection.SendAsync("UploadDataUri", dataUri, id1, id2, room.Motiondetection, room.Motiondetectionfluctation).ContinueWith((task) =>
                        {
                            if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                            {
                                if (this.TryHandleException(task))
                                    return;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }
        public async Task OnDownloadBytesExecuteAsync(string dataURI, string id, string camId)
        {
            try
            {
                await this.VideoService.AppendBufferRemoteLivestream(dataURI, id, camId);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }
        public async Task TookSnapshotEventExecute(string imageUri, string roomId, string camId, VideoSnapshotActivatorType snapshotActivatorType)
        {
            await this.Connection.InvokeAsync("UploadSnapshotUri", imageUri, Convert.ToInt32(roomId), snapshotActivatorType).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public void OnUpdateRoomCreatorExecute(int roomId, ChatHubUser creator)
        {
            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            if(room != null)
            {
                room.Creator = creator;
                this.RunUpdateUI();
            }
        }
        public async Task Onupdatebingmapexecute(int roomId, int connectionId, ChatHubGeolocation position)
        {
            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            if (room != null)
            {
                await this.Jsapibingmapservice.Renderbingmapposition(string.Concat("roomid", roomId, "connectionid", connectionId.ToString()), position.latitude, position.longitude);
                this.RunUpdateUI();
            }
        }
        public async Task GetChatHubViewers()
        {
            if (this.Rooms != null && this.Rooms.Any())
            {
                List<int> roomIds = this.Rooms.Select(item => item.Id).ToList<int>();
                await this.Connection.InvokeAsync<IList<ChatHubViewer>[]>("GetChatHubViewers", roomIds).ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted && task.Status == TaskStatus.RanToCompletion)
                    {
                        if (this.TryHandleException(task))
                            return;

                        try
                        {
                            IList<ChatHubViewer>[] result = task.Result;
                            if (result != null && result.Any())
                            {
                                foreach (var room in this.Rooms)
                                {
                                    var viewerList = result.FirstOrDefault(viewerList => viewerList.FirstOrDefault(item => item.RoomId == room.Id) != null);
                                    room.Viewers = viewerList ?? new List<ChatHubViewer>();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            this.HandleException(exception);
                        }
                    }
                });
            }
            
        }

        public async Task<ChatHubRoom> GetRoom(int roomId, int moduleId)
        {
            ChatHubRoom gotRoom = null;
            await this.Connection.InvokeAsync<ChatHubRoom>("GetRoom", roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    gotRoom = task.Result;
                }
            });

            return gotRoom;
        }
        public async Task<List<ChatHubRoom>> GetRoomsByModuleId(int moduleId)
        {
            List<ChatHubRoom> rooms = new List<ChatHubRoom>();
            await this.Connection.InvokeAsync<List<ChatHubRoom>>("GetRoomsByModuleId").ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    rooms = task.Result;
                }
            });

            return rooms;
        }
        public async Task<List<ChatHubRoom>> GetLobbiesByModuleId(int moduleId)
        {
            List<ChatHubRoom> rooms = new List<ChatHubRoom>();
            await this.Connection.InvokeAsync<List<ChatHubRoom>>("GetLobbiesByModuleId").ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    rooms = task.Result;
                }
            });

            return rooms;
        }
        public async Task<ChatHubRoom> CreateRoom(ChatHubRoom room)
        {
            ChatHubRoom createdRoom = null;
            await this.Connection.InvokeAsync<ChatHubRoom>("CreateRoom", room).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    createdRoom = task.Result;
                }
            });

            return createdRoom;
        }
        public async Task<ChatHubRoom> UpdateRoom(ChatHubRoom room)
        {
            ChatHubRoom updatedRoom = null;
            await this.Connection.InvokeAsync<ChatHubRoom>("UpdateRoom", room).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    updatedRoom = task.Result;
                }
            });

            return updatedRoom;
        }
        public async Task DeleteRoom(int roomId)
        {
            await this.Connection.InvokeAsync("DeleteRoom", roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task DeleteCam(int camId)
        {
            await this.Connection.InvokeAsync("DeleteCam", camId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }

        public async Task EnterChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("EnterChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task LeaveChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("LeaveChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task StartCam(VideoModel model)
        {
            await this.Connection.InvokeAsync("StartCam", Convert.ToInt32(model.Id1), Convert.ToInt32(model.Id2)).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });            
        }
        public async Task StopCam(VideoModel model)
        {
            await this.Connection.InvokeAsync("StopCam", Convert.ToInt32(model.Id1), Convert.ToInt32(model.Id2)).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }

        public async Task RemoveInvitation(ChatHubInvitation invitation)
        {
            await this.Connection.SendAsync("RemoveInvitation", invitation.Id).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task SendMessage(string content, int roomId)
        {
            await this.Connection.InvokeAsync("SendMessage", content, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task EnterRoom_Clicked(int roomId, int moduleid)
        {
            if (!this.Rooms.Any(item => item.Id == roomId) && this.Connection?.State == HubConnectionState.Connected)
            {
                await this.EnterChatRoom(roomId);
            }
        }
        public async Task EnableArchiveRoom_Clicked(ChatHubRoom room)
        {
            try
            {
                if (room.Status == ChatHubRoomStatus.Archived.ToString())
                {
                    room.Status = ChatHubRoomStatus.Enabled.ToString();
                }
                else if (room.Status == ChatHubRoomStatus.Enabled.ToString())
                {
                    room.Status = ChatHubRoomStatus.Archived.ToString();
                }

                await this.UpdateRoom(room);
            }
            catch
            {
                this.BlazorAlertsService.NewBlazorAlert("Could not enable/archive room.");
            }
        }
        public async Task DeleteRoom_Clicked(int id)
        {
            try
            {
                await this.DeleteRoom(id);
            }
            catch
            {
                this.BlazorAlertsService.NewBlazorAlert("Could not delete room.");
            }
        }
        public async Task FollowInvitation_Clicked(int invitationId, int roomId)
        {
            if (this.Connection?.State == HubConnectionState.Connected)
            {
                await this.EnterChatRoom(roomId);
                this.Invitations.RemoveInvitation(invitationId);
                this.RunUpdateUI();
            }
        }

        public void IgnoreUser_Clicked(int targetUserId)
        {
            this.Connection.SendAsync("IgnoreUser", targetUserId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task UnignoreUser(int targetUserId)
        {
            await this.Connection.SendAsync("UnignoreUser", targetUserId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public void AddModerator_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddModerator", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task RemoveModeratorAsync(int userId, int roomId)
        {
            await this.Connection.InvokeAsync("RemoveModerator", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public void AddWhitelistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddWhitelistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task RemoveWhitelistUser_Clicked(int userId, int roomId)
        {
            await this.Connection.InvokeAsync("RemoveWhitelistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public void AddBlacklistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddBlacklistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public async Task RemoveBlacklistUser_Clicked(int userId, int roomId)
        {
            await this.Connection.InvokeAsync("RemoveBlacklistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        public void RemoveWaitingRoomItem_Clicked(ChatHubWaitingRoomItem waitingRoomItem)
        {
            this.AddWhitelistUser_Clicked(waitingRoomItem.UserId, waitingRoomItem.RoomId);

            this.Connection.InvokeAsync("RemoveWaitingRoomItem", waitingRoomItem).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });

            this.Rooms.FirstOrDefault(item => item.Id == waitingRoomItem.RoomId).WaitingRoomItems.Remove(waitingRoomItem);
            this.RunUpdateUI();
        }

        public async Task<ChatHubDevice> GetDefaultDevice(string roomId, ChatHubDeviceType type)
        {
            ChatHubDevice device = null;

            if (this.Connected)
            {
                await this.Connection.InvokeAsync<ChatHubDevice>("GetDefaultDevice", Convert.ToInt32(roomId), type).ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                    {
                        if (this.TryHandleException(task))
                            return;

                        device = task.Result;
                    }
                });
            }

            return device;
        }

        private async Task OnAudioDeviceChanged(DevicesEvent e)
        {
            await this.Connection.InvokeAsync("SetDefaultDevice", Convert.ToInt32(e.Id), e.Item.id, e.Item.name, ChatHubDeviceType.Audio).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        private async Task OnMicrophoneDeviceChanged(DevicesEvent e)
        {
            await this.Connection.InvokeAsync("SetDefaultDevice", Convert.ToInt32(e.Id), e.Item.id, e.Item.name, ChatHubDeviceType.Microphone).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }
        private async Task OnWebcamDeviceChanged(DevicesEvent e)
        {
            await this.Connection.InvokeAsync("SetDefaultDevice", Convert.ToInt32(e.Id), e.Item.id, e.Item.name, ChatHubDeviceType.Webcam).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }

        private void OnAddChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.Rooms.AddRoom(room);
            if(string.IsNullOrEmpty(this.ContextRoomId))
            {
                this.ShowWindow(room.Id.ToString());
            }

            NotificationItem notification = new NotificationItem()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Notification",
                Content = String.Concat("Entered room ", room.Title, "."),
                Type = NotificationType.Success,
            };

            this.NotificationsService.AddNotification(notification);
            this.RunUpdateUI();
        }
        private void OnRemoveChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.Rooms.RemoveRoom(room);

            if (!this.Rooms.Any(item => item.Id.ToString() == this.ContextRoomId))
            {
                this.ShowWindow(this.Rooms.FirstOrDefault().Id.ToString());
            }
            else
            {
                this.ShowWindow(this.ContextRoomId);
            }

            this.RunUpdateUI();
        }
        private void OnAddChatHubUserExecute(ChatHubUser user, string roomId)
        {
            this.Rooms.AddUser(user, roomId);
            this.RunUpdateUI();
        }
        private void OnRemoveChatHubUserExecute(ChatHubUser user, string roomId)
        {
            this.Rooms.RemoveUser(user, roomId);
            this.RunUpdateUI();
        }
        public void OnAddChatHubMessageExecute(object sender, ChatHubMessage message)
        {
            ChatHubRoom room = this.Rooms.FirstOrDefault(item => item.Id == message.ChatHubRoomId);
            room.AddMessage(message);

            if (message.ChatHubRoomId.ToString() != this.ContextRoomId)
            {
                this.Rooms.FirstOrDefault(room => room.Id == message.ChatHubRoomId).UnreadMessages++;
            }

            this.RunUpdateUI();

            string elementId = string.Concat("#message-window-", this.ModuleId.ToString(), "-", message.ChatHubRoomId.ToString());
            this.ScrollService.ScrollToBottom(elementId);
        }
        private void OnAddChatHubWaitingRoomItemExecute(object sender, ChatHubWaitingRoomItem e)
        {
            this.Rooms.FirstOrDefault(item => item.Id == e.RoomId).WaitingRoomItems.Add(e);
        }
        private async void OnRemovedChathubWaitingRoomItemExecute(object sender, ChatHubWaitingRoomItem e)
        {
            var room = await this.GetRoom(e.RoomId, this.ModuleId);
            this.BlazorAlertsService.NewBlazorAlert($"You have been granted access to the room named {room.Title}. Do you like to enter??", "Javascript Application", PositionType.Fixed, true, e.RoomId.ToString());
        }
        private void OnAddChatHubCamExecute(ChatHubCam cam, int roomId)
        {
            this.Rooms.AddCam(cam, roomId);
            this.RunUpdateUI();
        }
        private void OnRemoveChatHubCamExecute(ChatHubCam cam, int roomId)
        {
            this.Rooms.RemoveCam(cam, roomId);
            this.RunUpdateUI();
        }
        private void OnClearHistoryExecute(object sender, int roomId)
        {
            this.ClearHistory(roomId);
        }
        public void OnUpdateConnectedUserExecute(object sender, ChatHubUser user)
        {
            this.ConnectedUser = user;
            this.RunUpdateUI();
        }
        public async void OnAlertConfirmedExecute(object sender, dynamic obj)
        {
            bool confirmed = (bool)obj.confirmed;
            BlazorAlertsModel model = (BlazorAlertsModel)obj.model;

            if (confirmed)
            {
                await this.EnterChatRoom(Convert.ToInt32(model.Id));
            }
        }

        private async Task OnGetNextSequenceExecute(VideoPlayerModel obj)
        {
            await this.Connection.InvokeAsync<VideoPlayerApiItem>("DowloadDataUri", obj.ParameterId1, obj.ParameterId2, obj.LastSequenceId, obj.SliderCurrentValue, obj.SliderValueChanged).ContinueWith(async (task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;

                    VideoPlayerApiItem apiItem = task.Result;
                    if (apiItem != null)
                        await this.VideoPlayerService.AddVideoSequence(obj.MapId, apiItem);
                }
            });
        }       
        private async Task OnGetViewersTimerElapsed(object source, ElapsedEventArgs e)
        {
            await this.GetChatHubViewers();
        }

        public async Task DisconnectAsync()
        {
            if (this.Connection != null && this.Connection.State != HubConnectionState.Disconnected)
            {
                await this.Connection.StopAsync();
            }
        }
        public void ClearHistory(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(x => x.Id == roomId);
            room.Messages.Clear();
            this.RunUpdateUI();
        }
        public void ToggleUserlist(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            if(room != null)
            {
                room.ShowUserlist = !room.ShowUserlist;
                this.RunUpdateUI();
            }
        }
        public string AutocompleteUsername(string msgInput, int roomId, int autocompleteCounter, string pressedKey)
        {
            List<string> words = msgInput.Trim().Split(' ').ToList();
            string lastWord = words.Last();

            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            var users = room.Users.Where(x => x.DisplayName.StartsWith(lastWord));

            if (users.Any())
            {
                autocompleteCounter = autocompleteCounter % users.Count();

                words.Remove(lastWord);
                if (pressedKey == "Enter")
                    words.Add(users.ToArray()[autocompleteCounter].DisplayName);

                msgInput = string.Join(' ', words);
            }

            return msgInput;
        }
        public void ShowWindow(string id)
        {
            this.ContextRoomId = id;
            var room = this.Rooms.FirstOrDefault(item => item.Id.ToString() == this.ContextRoomId);
            if (room != null)
            {
                room.UnreadMessages = 0;
            }
            this.RunUpdateUI();
        }

        public bool TryHandleException(Task task)
        {
            if (task.Exception != null)
            {
                this.HandleException(task.Exception);
            }

            return false;
        }
        public void HandleException(Exception exception)
        {
            string message = string.Empty;
            if (exception.InnerException != null && exception.InnerException is HubException)
            {
                message = exception.Message.ToString();
            }
            else
            {
                message = exception.ToString();
            }

            var item = new NotificationItem() { Id = Guid.NewGuid().ToString(), Title = "Notification", Content = message };
            this.NotificationsService.AddNotification(item);
            this.RunUpdateUI();
        }

        public string ChatHubControllerApiUrl
        {
            get { return CreateApiUrl(SiteState.Alias, "ChatHub"); }
        }
        public async Task ArchiveActiveDbItems(int ModuleId)
        {
            await this.Connection.InvokeAsync("ArchiveActiveDbItems").ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    this.TryHandleException(task);
                }
            });
        }
        public async Task CreateExampleData()
        {
            await this.Connection.InvokeAsync("CreateExampleData").ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    this.TryHandleException(task);
                    this.BlazorAlertsService.NewBlazorAlert("Wait for it maybe a minute. Your data will be created.");
                }
            });
        }
        public async Task CreateExampleUserData(int roomId)
        {
            await this.Connection.InvokeAsync("CreateExampleUserData", roomId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    this.TryHandleException(task);
                }
            });
        }

        public ChatHubCam GetCamByRoom(ChatHubRoom context, string connectionId)
        {
            var activeConnectionModel = context.Creator.Connections.FirstOrDefault(item => item.ConnectionId == connectionId);
            if (activeConnectionModel != null)
            {
                return context.Cams.FirstOrDefault(item => item.ChatHubConnectionId == activeConnectionModel.Id);
            }

            return null;
        }
        public async Task DownloadVideo(int moduleId, string connectionId, int roomId, int camId)
        {
            var apiUri = string.Concat(NavigationManager.BaseUri.Substring(0, NavigationManager.BaseUri.LastIndexOf('/')), this.ChatHubControllerApiUrl, "/DownloadVideo");
            var httpResponseMessage = await this.HttpClient.GetAsync(apiUri + "/" + moduleId + "/" + connectionId + "/" + roomId +  "/" + camId);
            var dataUri = await httpResponseMessage.Content.ReadAsStringAsync();

            var fileName = Guid.NewGuid().ToString() + ".mp4";
            await this.chatHubMap.InvokeVoidAsync("downloadcapturedvideoitem", fileName, dataUri);
        }

        public ChatHubVisitorsDisplay Display { get; set; } = new ChatHubVisitorsDisplay() { Items = null };
        public async Task GetVisitorsDisplay(int moduleId)
        {
            await this.Connection.InvokeAsync<ChatHubVisitorsDisplay>("GetVisitorsDisplay", moduleId).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                    this.Display = task.Result;
                    this.RunUpdateUI();
                }
            });
        }

        public async Task GeolocationreceivedAsync(Jsapigeolocationpositionevent obj)
        {
            ChatHubGeolocation position = new ChatHubGeolocation();

            position.state = obj.permissionstate;
            position.latitude = obj.Item.latitude;
            position.longitude = obj.Item.longitude;
            position.altitude = obj.Item.altitude;
            position.altitudeaccuracy = obj.Item.altitudeaccuracy;
            position.accuracy = obj.Item.accuracy;
            position.heading = obj.Item.heading;
            position.speed = obj.Item.speed;

            await this.Connection.InvokeAsync("AddGeolocationPosition", position).ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted)
                {
                    if (this.TryHandleException(task))
                        return;
                }
            });
        }

        public void RunUpdateUI()
        {
            this.OnUpdateUI.Invoke(this, EventArgs.Empty);
        }
        public void Dispose()
        {
            this.VideoService.StartVideoEvent -= async (VideoModel model) => await this.StartCam(model);
            this.VideoService.StopVideoEvent -= async (VideoModel model) => await this.StopCam(model);
            this.VideoService.VideoServiceExtension.OnDataAvailableEventHandler -= async (string data, string id1, string id2) => await OnDataAvailableEventHandlerExecute(data, id1, id2);
            this.VideoService.TookSnapshotEvent -= async (string imageUri, string roomId, string camId, VideoSnapshotActivatorType snapshotActivatorType) => await this.TookSnapshotEventExecute(imageUri, roomId, camId, snapshotActivatorType);

            this.VideoPlayerService.VideoServiceExtension.OnGetNextSequence -= async (VideoPlayerModel item) => await OnGetNextSequenceExecute(item);

            this.BlazorAlertsService.OnAlertConfirmed -= OnAlertConfirmedExecute;

            this.OnUpdateConnectedUserEvent -= OnUpdateConnectedUserExecute;
            this.OnAddChatHubRoomEvent -= OnAddChatHubRoomExecute;
            this.OnRemoveChatHubRoomEvent -= OnRemoveChatHubRoomExecute;
            this.OnAddChatHubUserEvent -= OnAddChatHubUserExecute;
            this.OnRemoveChatHubUserEvent -= OnRemoveChatHubUserExecute;
            this.OnAddChatHubMessageEvent -= OnAddChatHubMessageExecute;
            this.OnAddChatHubWaitingRoomItemEvent -= OnAddChatHubWaitingRoomItemExecute;
            this.OnRemovedChatHubWaitingRoomItemEvent -= OnRemovedChathubWaitingRoomItemExecute;
            this.OnAddChatHubCamEvent -= OnAddChatHubCamExecute;
            this.OnRemoveChatHubCamEvent -= OnRemoveChatHubCamExecute;
            this.OnDownloadBytesEvent -= async (string dataURI, string id, string camId) => await OnDownloadBytesExecuteAsync(dataURI, id, camId);
            this.OnUpdateRoomCreatorEvent -= OnUpdateRoomCreatorExecute;
            this.Onupdatebingmapevent -= async (int roomId, int connectionId, ChatHubGeolocation position) => await Onupdatebingmapexecute(roomId, connectionId, position);
            this.OnClearHistoryEvent -= OnClearHistoryExecute;
            this.OnMatchedEvent -= MatchedEventExecute;

            this.Jsapigeolocationservice.OnGeolocationpositionReceived -= async (Jsapigeolocationpositionevent item) => await GeolocationreceivedAsync(item);

            this.DevicesService.OnAudioDeviceChanged -= async (DevicesEvent e) => await OnAudioDeviceChanged(e);
            this.DevicesService.OnMicrophoneDeviceChanged -= async (DevicesEvent e) => await OnMicrophoneDeviceChanged(e);
            this.DevicesService.OnWebcamDeviceChanged -= async (DevicesEvent e) => await OnWebcamDeviceChanged(e);

            this.GetViewersTimer.Elapsed -= new ElapsedEventHandler(async (object source, ElapsedEventArgs e) => await OnGetViewersTimerElapsed(source, e));
        }

    }
}
