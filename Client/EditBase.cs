using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared;
using Hwavmvid.Alerts;
using System.Collections.Generic;
using BlazorSelect;
using Hwavmvid.ColorPicker;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;

namespace Oqtane.ChatHubs
{
    public class EditBase : ModuleBase, IDisposable
    {

        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ChatHubService ChatHubService { get; set; }
        [Inject] public AlertsService AlertsService { get; set; }
        [Inject] public ColorPickerService ColorPickerService { get; set; }

        protected readonly string FileUploadDropzoneContainerElementId = "EditComponentFileUploadDropzoneContainer";
        protected readonly string FileUploadInputFileElementId = "EditComponentFileUploadInputFileContainer";

        public HashSet<string> SelectionItems { get; set; } = new HashSet<string>();

        public ColorPickerType ColorPickerActiveType { get; set; }

        public HashSet<string> ColorPickerSelectionItems { get; set; } = new HashSet<string>();

        public override SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.Anonymous; } }
        public override string Actions { get { return "Add,Edit"; } }

        public int roomId = -1;
        public string title;
        public string content;
        public string backgroundcolor;
        public string type;
        public string imageUrl;
        public string createdby;
        public DateTime createdon;
        public string modifiedby;
        public DateTime modifiedon;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.ColorPickerSelectionItems.Add(ColorPickerType.HTML5ColorPicker.ToString());
                this.ColorPickerSelectionItems.Add(ColorPickerType.CustomColorPicker.ToString());
                this.ColorPickerActiveType = ColorPickerType.CustomColorPicker;

                this.SelectionItems.Add(ChatHubRoomType.Public.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Protected.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Private.ToString());

                this.type = ChatHubRoomType.Public.ToString();

                this.ColorPickerService.OnColorPickerContextColorChangedEvent += OnColorPickerContextColorChangedExecute;

                this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUIStateHasChanged();
                await this.InitContextRoomAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        private void OnColorPickerContextColorChangedExecute(ColorPickerEvent obj)
        {
            this.backgroundcolor = obj.ContextColor;
            this.UpdateUIStateHasChanged();
        }

        public void OnSelect(BlazorSelectEvent e)
        {
            this.type = e.SelectedItem;
            this.UpdateUIStateHasChanged();
        }

        public void ColorPicker_OnSelect(BlazorSelectEvent e)
        {
            this.ColorPickerActiveType = (ColorPickerType)Enum.Parse(typeof(ColorPickerType), e.SelectedItem, true);
            this.UpdateUIStateHasChanged();
        }

        private async Task InitContextRoomAsync()
        {
            try
            {
                if (PageState.QueryString.ContainsKey("roomid"))
                {
                    this.roomId = Int32.Parse(PageState.QueryString["roomid"]);
                    ChatHubRoom room = await this.ChatHubService.GetRoom(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        this.title = room.Title;
                        this.content = room.Content;
                        this.backgroundcolor = room.BackgroundColor;
                        this.type = room.Type;
                        this.imageUrl = room.ImageUrl;
                        this.createdby = room.CreatedBy;
                        this.createdon = room.CreatedOn;
                        this.modifiedby = room.ModifiedBy;
                        this.modifiedon = room.ModifiedOn;
                    }
                    else
                    {
                        await logger.LogError("Error Loading Room {ChatHubRoomId} {Error}", roomId);
                        ModuleInstance.AddModuleMessage("Error Loading ChatHub", MessageType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        public async Task SaveRoom()
        {
            try
            {                
                if (roomId == -1)
                {
                    ChatHubRoom room = new ChatHubRoom()
                    {
                        ModuleId = ModuleState.ModuleId,
                        Title = this.title,
                        Content = this.content,
                        BackgroundColor = this.backgroundcolor,
                        Type = this.type,
                        Status = ChatHubRoomStatus.Enabled.ToString(),
                        ImageUrl = string.Empty,
                        OneVsOneId = string.Empty,
                        CreatorId = ChatHubService.ConnectedUser.UserId,
                    };

                    room = await this.ChatHubService.CreateRoom(room);
                    await logger.LogInformation("Room Added {ChatHubRoom}", room);
                    NavigationManager.NavigateTo(NavigateUrl());
                }
                else
                {                    
                    ChatHubRoom room = await this.ChatHubService.GetRoom(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        room.Title = this.title;
                        room.Content = this.content;
                        room.BackgroundColor = this.backgroundcolor;
                        room.Type = this.type;

                        await this.ChatHubService.UpdateRoom(room);

                        await logger.LogInformation("Room Updated {ChatHubRoom}", room);
                        NavigationManager.NavigateTo(NavigateUrl());
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Saving Room", MessageType.Error);
            }
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.ColorPickerService.OnColorPickerContextColorChangedEvent -= OnColorPickerContextColorChangedExecute;
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUIStateHasChanged();
        }

    }
}
