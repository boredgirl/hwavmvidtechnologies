using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorAlerts;
using System.Net;
using BlazorDraggableList;
using BlazorFileUpload;
using BlazorBrowserResize;
using BlazorVideo;
using Oqtane.ChatHubs.Models;
using BlazorModal;
using Oqtane.Models;
using BlazorDropdown;
using BlazorNotifications;
using BlazorPager;
using BlazorDynamicLayout;
using Hwavmvid.Jsapinotifications;

namespace Oqtane.ChatHubs
{
    public class IndexBase : ModuleBase, IDisposable
    {
        
        [Inject] protected IJSRuntime JsRuntime { get; set; }
        [Inject] protected ISettingService SettingService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }
        [Inject] protected SiteState SiteState { get; set; }
        [Inject] protected BlazorAlertsService BlazorAlertsService { get; set; }
        [Inject] protected ChatHubService ChatHubService { get; set; }
        [Inject] protected BlazorBrowserResizeService BrowserResizeService { get; set; }
        [Inject] protected ScrollService ScrollService { get; set; }
        [Inject] protected CookieService CookieService { get; set; }
        [Inject] protected BlazorDraggableListService BlazorDraggableListService { get; set; }
        [Inject] protected BlazorFileUploadService BlazorFileUploadService { get; set; }
        [Inject] protected BlazorVideoService BlazorVideoService { get; set; }
        [Inject] protected BlazorModalService BlazorModalService { get; set; }
        [Inject] protected BlazorDynamicLayoutService BlazorDynamicLayoutService { get; set; }
        [Inject] protected JsapinotificationService JsapinotificationService { get; set; }
        public ChatHubRoom contextRoom { get; set; }

        public string GuestUsername { get; set; } = string.Empty;
        public int MessageWindowHeight { get; set; }
        public int UserlistWindowHeight { get; set; }
        public string BackgroundColor { get; set; }

        public int maxUserNameCharacters { get; set; }
        public int framerate { get; set; }
        public int videoBitsPerSecond { get; set; }
        public int audioBitsPerSecond { get; set; }
        public int videoSegmentsLength { get; set; }

        public int InnerHeight = 0;
        public int InnerWidth = 0;

        public Dictionary<string, string> settings { get; set; }

        public ImageModal ImageModalRef;
        public SettingsModal SettingsModalRef;
        public EditRoomModal EditRoomModalRef;

        protected readonly string DraggableLivestreamContainerElementId = "DraggableLivestreamsContainer";

        protected override async Task OnInitializedAsync()
        {
            this.ChatHubService.ModuleId = ModuleState.ModuleId;

            Dictionary<string, string> settings = await this.SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
            this.BackgroundColor = this.SettingService.GetSetting(settings, "BackgroundColor", "#fff0f0");
            this.maxUserNameCharacters = Int32.Parse(this.SettingService.GetSetting(settings, "MaxUserNameCharacters", "20"));
            this.framerate = Int32.Parse(this.SettingService.GetSetting(settings, "Framerate", "24"));
            this.videoBitsPerSecond = Int32.Parse(this.SettingService.GetSetting(settings, "VideoBitsPerSecond", "14000"));
            this.audioBitsPerSecond = Int32.Parse(this.SettingService.GetSetting(settings, "AudioBitsPerSecond", "12800"));
            this.videoSegmentsLength = Int32.Parse(this.SettingService.GetSetting(settings, "VideoSegmentsLength", "2400"));

            this.BlazorDynamicLayoutService.TabItemClickedEvent += OnTabItemClickedExecute;
            this.BlazorDynamicLayoutService.OnErrorEvent += OnBlazorDynamicLayoutErrorExecute;
            this.BlazorVideoService.OnError += OnBlazorVideoErrorExecute;
            this.BrowserResizeService.BrowserResizeServiceExtension.OnResize += BrowserHasResized;
            this.BlazorDraggableListService.BlazorDraggableListServiceExtension.OnDropEvent += OnDraggableListDropEventExecute;
            this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUI();

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            try
            {
                if (PageState.QueryString.ContainsKey("moduleid") && PageState.QueryString.ContainsKey("roomid") && int.Parse(PageState.QueryString["moduleid"]) == ModuleState.ModuleId)
                {
                    this.contextRoom = await this.ChatHubService.GetRoom(int.Parse(PageState.QueryString["roomid"]), ModuleState.ModuleId);
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Rooms {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Rooms", MessageType.Error);
            }

            await base.OnParametersSetAsync();
        }        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.ChatHubService.InitChatHubService();
                await this.CookieService.InitCookieService();
                await this.ScrollService.InitScrollService();
                await this.BrowserResizeService.InitBrowserResizeService();
                await this.BlazorModalService.InitBlazorModal();
                await this.JsapinotificationService.InitJsapinotifications();

                string hostname = new Uri(NavigationManager.BaseUri).Host;
                string cookievalue = await this.CookieService.GetCookieAsync(".AspNetCore.Identity.Application");
                this.ChatHubService.IdentityCookie = new Cookie(".AspNetCore.Identity.Application", cookievalue, "/", hostname);

                await this.ChatHubService.ConnectToChat(this.GuestUsername, ModuleState.ModuleId);
                await this.ChatHubService.chatHubMap.InvokeVoidAsync("showchathubscontainer");

                await this.BrowserResizeService.RegisterWindowResizeCallback();
                await BrowserHasResized();

                await base.OnAfterRenderAsync(firstRender);

                Stringpics.StringpicsItellisense itellisense = new Stringpics.StringpicsItellisense();
                string consoleitem = itellisense.GetStringPic("car", Stringpics.StringpicsOutputType.console);
                await this.ChatHubService.ConsoleLog(consoleitem);

                bool granted = await this.JsapinotificationService.RequestPermission();
                if (granted)
                    await this.JsapinotificationService.ShowNotification(new Jsapinotification()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "App Notifications enabled",
                        Dir = "auto",
                        Lang = "en-US",
                        Body = consoleitem,
                        Tag = "hwavmvid",
                        Icon = string.Empty,
                        Data = string.Empty,
                    });
            }
        }

        private async void OnDraggableListDropEventExecute(object sender, BlazorDraggableListEvent e)
        {
            try
            {
                if (this.DraggableLivestreamContainerElementId == e.DraggableContainerElementId)
                {
                    List<ChatHubRoom> rooms = new List<ChatHubRoom>();
                    rooms.Add(this.ChatHubService.Rooms[e.DraggableItemOldIndex]);
                    rooms.Add(this.ChatHubService.Rooms[e.DraggableItemNewIndex]);

                    foreach (var room in rooms)
                    {
                        if (ChatHubService.ConnectedUser?.UserId == room.CreatorId)
                        {
                            var activeCamModel = ChatHubService.GetCamByRoom(room, ChatHubService.Connection.ConnectionId);
                            if (activeCamModel != null)
                                await this.BlazorVideoService.RestartStreamTaskIfExists(room.Id.ToString(), activeCamModel.Id.ToString());
                        }
                        else
                        {
                            if (room.Creator != null)
                            {
                                foreach (var connection in room.Creator?.Connections)
                                {
                                    var activeCamModel = ChatHubService.GetCamByRoom(room, connection.ConnectionId);
                                    if (activeCamModel != null)
                                        await this.BlazorVideoService.RestartStreamTaskIfExists(room.Id.ToString(), activeCamModel.Id.ToString());
                                }
                            }
                        }
                    }

                    this.UpdateUI();
                }
            }
            catch
            {
                this.ChatHubService.BlazorNotificationsService.AddNotification(new NotificationItem() { Id = Guid.NewGuid().ToString(), Content = "Failed swap room item.", Type = BlazorNotificationType.Danger });
            }
        }        
        public async Task LeaveRoom_Clicked(int roomId, int moduleId)
        {
            await this.ChatHubService.LeaveChatRoom(roomId);
            this.RemovedWindow();
        }        
        private async Task BrowserHasResized()
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    this.InnerHeight = await this.BrowserResizeService.GetInnerHeight();
                    this.InnerWidth = await this.BrowserResizeService.GetInnerWidth();

                    this.MessageWindowHeight = 520;
                    this.UserlistWindowHeight = 570;

                    StateHasChanged();
                });
            }
            catch(Exception ex)
            {
                await logger.LogError(ex, "Error On Browser Resize {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error On Browser Resize", MessageType.Error);
            }
        }
        public void UserlistItem_Clicked(MouseEventArgs e, ChatHubRoom room, ChatHubUser user)
        {
            InvokeAsync(() =>
            {
                if (user.UserlistItemCollapsed)
                {
                    user.UserlistItemCollapsed = false;
                }
                else
                {
                    foreach (var chatUser in room.Users.Where(x => x.UserlistItemCollapsed == true))
                    {
                        chatUser.UserlistItemCollapsed = false;
                    }
                    user.UserlistItemCollapsed = true;
                }

                StateHasChanged();
            });
        }
        public void SettingsDropdown_Clicked(BlazorDropdownEvent e)
        {
            this.ChatHubService.ContextRoomId = e.ClickedDropdownItem.Id.ToString();
            this.ChatHubService.ToggleUserlist(e.ClickedDropdownItem.Id);
            this.UpdateUI();
        }
        private void OnTabItemClickedExecute(TabItemEvent tabEvent)
        {
            this.ChatHubService.ShowWindow(tabEvent.ActivatedItemId);
        }

        private void OnBlazorDynamicLayoutErrorExecute(string message)
        {
            this.ChatHubService.BlazorNotificationsService.AddNotification(
                new NotificationItem() { 
                    Id = Guid.NewGuid().ToString(),
                    Title = "Notification",
                    Content = message,
                    Type = BlazorNotificationType.Danger });
        }
        private void OnBlazorVideoErrorExecute(string message)
        {
            this.ChatHubService.BlazorNotificationsService.AddNotification(
                new NotificationItem() { 
                    Id = Guid.NewGuid().ToString(), 
                    Title = "Notification", 
                    Content = message, 
                    Type = BlazorNotificationType.Danger });
        }

        public async void RemovedWindow()
        {
            foreach (var room in this.ChatHubService.Rooms)
            {
                if (ChatHubService.ConnectedUser?.UserId == room.CreatorId)
                {
                    var activeCamModel = ChatHubService.GetCamByRoom(room, ChatHubService.Connection.ConnectionId);
                    if (activeCamModel != null)
                        await this.BlazorVideoService.RestartStreamTaskIfExists(room.Id.ToString(), activeCamModel.Id.ToString());
                }
                else
                {
                    if (room.Creator != null)
                    {
                        foreach (var connection in room.Creator?.Connections)
                        {
                            var activeCamModel = ChatHubService.GetCamByRoom(room, connection.ConnectionId);
                            if (activeCamModel != null)
                                await this.BlazorVideoService.RestartStreamTaskIfExists(room.Id.ToString(), activeCamModel.Id.ToString());
                        }
                    }
                }
            }
        }

        public override List<Resource> Resources => new List<Resource>()
        {
            new Resource { ResourceType = ResourceType.Script, Bundle = "jQuery", Url = "https://code.jquery.com/jquery-3.2.1.slim.min.js", Integrity = "sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN", CrossOrigin = "anonymous", Location = ResourceLocation.Body, Declaration = ResourceDeclaration.Local },
            new Resource { ResourceType = ResourceType.Script, Bundle = "IoButtons", Url = "https://buttons.github.io/buttons.js", CrossOrigin = "anonymous", Location = ResourceLocation.Body, Declaration = ResourceDeclaration.Local },
            new Resource { ResourceType = ResourceType.Script, Bundle = "Popper", Url = "https://cdn.jsdelivr.net/npm/@popperjs/core@2.9.2/dist/umd/popper.min.js", Integrity = "sha384-IQsoLXl5PILFhosVNubq5LC7Qb9DXgDA9i+tQ8Zj3iwWAwPtgFTxbJ8NT4GN1R8p", CrossOrigin = "anonymous", Location = ResourceLocation.Body, Declaration = ResourceDeclaration.Local },
        };
        public async void UpdateUI()
        {
            await InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }
        public void Dispose()
        {
            this.BlazorDynamicLayoutService.TabItemClickedEvent -= OnTabItemClickedExecute;
            this.BlazorDynamicLayoutService.OnErrorEvent -= OnBlazorDynamicLayoutErrorExecute;
            this.BlazorVideoService.OnError -= OnBlazorVideoErrorExecute;
            this.BlazorDraggableListService.BlazorDraggableListServiceExtension.OnDropEvent -= OnDraggableListDropEventExecute;
            this.BrowserResizeService.BrowserResizeServiceExtension.OnResize -= BrowserHasResized;
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUI();

            if (ChatHubService.Connection != null)
            {
                this.ChatHubService.Connection.StopAsync();
                this.ChatHubService.Connection.DisposeAsync();
            }            
        }
        
    }

    public static class IndexBaseExtensionMethods
    {
        public static IList<ChatHubRoom> Swap<TItemGeneric>(this IList<ChatHubRoom> list, int x, int y)
        {
            ChatHubRoom temp = list[x];
            list[x] = list[y];
            list[y] = temp;
            return list;
        }
    }

}