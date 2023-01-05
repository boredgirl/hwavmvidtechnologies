﻿using BlazorAlerts;
using BlazorColorPicker;
using BlazorModal;
using BlazorSelect;
using BlazorNotifications;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Enums;
using Oqtane.ChatHubs.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorDevices;

namespace Oqtane.ChatHubs
{
    public class EditRoomModalBase : ModuleBase, IDisposable
    {

        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public SiteState SiteState { get; set; }
        [Inject] public ChatHubService ChatHubService { get; set; }
        [Inject] public BlazorAlertsService BlazorAlertsService { get; set; }
        [Inject] public BlazorColorPickerService BlazorColorPickerService { get; set; }
        [Inject] public BlazorModalService BlazorModalService { get; set; }
        [Inject] public BlazorNotificationsService BlazorNotificationService { get; set; }

        public const string EditRoomModalElementId = "EditRoomModalElementId";

        protected readonly string FileUploadDropzoneContainerElementId = "EditComponentFileUploadDropzoneContainer";
        protected readonly string FileUploadInputFileElementId = "EditComponentFileUploadInputFileContainer";

        public HashSet<string> SelectionItems { get; set; } = new HashSet<string>();
        public HashSet<string> BlazorColorPickerSelectionItems { get; set; } = new HashSet<string>();

        public BlazorColorPickerType BlazorColorPickerActiveType { get; set; }

        public int roomId = -1;
        public string title;
        public string content;
        public string backgroundcolor;
        public string type;
        public string imageUrl;
        public string createdby;
        public string modifiedby;
        public DateTime createdon;
        public DateTime modifiedon;

        public BlazorDeviceItem AudioDefaultDevice { get; set; }
        public BlazorDeviceItem MicrophoneDefaultDevice { get; set; }
        public BlazorDeviceItem WebcamDefaultDevice { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.BlazorColorPickerSelectionItems.Add(BlazorColorPickerType.HTML5ColorPicker.ToString());
                this.BlazorColorPickerSelectionItems.Add(BlazorColorPickerType.CustomColorPicker.ToString());
                this.BlazorColorPickerActiveType = BlazorColorPickerType.CustomColorPicker;

                this.SelectionItems.Add(ChatHubRoomType.Public.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Protected.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Private.ToString());

                this.type = ChatHubRoomType.Public.ToString();

                this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent += OnBlazorColorPickerContextColorChangedExecute;
                this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUIStateHasChanged();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        private async Task GetDefaultDevices()
        {
            var audioDevice = await this.ChatHubService.GetDefaultDevice(this.roomId.ToString(), ChatHubDeviceType.Audio);
            if (audioDevice != null)
                this.AudioDefaultDevice = new BlazorDeviceItem() { id = audioDevice.DefaultDeviceId, name = audioDevice.DefaultDeviceName };
            else
                this.AudioDefaultDevice = new BlazorDeviceItem() { id = String.Empty, name = String.Empty };

            var microphoneDevice = await this.ChatHubService.GetDefaultDevice(this.roomId.ToString(), ChatHubDeviceType.Microphone);
            if (microphoneDevice != null)
                this.MicrophoneDefaultDevice = new BlazorDeviceItem() { id = microphoneDevice.DefaultDeviceId, name = microphoneDevice.DefaultDeviceName };
            else
                this.MicrophoneDefaultDevice = new BlazorDeviceItem() { id = String.Empty, name = String.Empty };

            var webcamDevice = await this.ChatHubService.GetDefaultDevice(this.roomId.ToString(), ChatHubDeviceType.Webcam);
            if (webcamDevice != null)
                this.WebcamDefaultDevice = new BlazorDeviceItem() { id = webcamDevice.DefaultDeviceId, name = webcamDevice.DefaultDeviceName };
            else
                this.WebcamDefaultDevice = new BlazorDeviceItem() { id = String.Empty, name = String.Empty };
        }

        public void InitCreateRoom()
        {
            this.roomId = -1;
            this.title = string.Empty;
            this.content = string.Empty;
            this.backgroundcolor = string.Empty;
            this.type = ChatHubRoomType.Public.ToString();
            this.imageUrl = string.Empty;
            this.createdby = string.Empty;
            this.createdon = DateTime.Now;
            this.modifiedby = string.Empty;
            this.modifiedon = DateTime.Now;
        }
        public async Task CreateRoom()
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
                SnapshotUrl = string.Empty,
                OneVsOneId = string.Empty,
                CreatorId = ChatHubService.ConnectedUser.UserId,
            };

            room = await this.ChatHubService.CreateRoom(room);
            await this.CloseModal();
            var item = new NotificationItem() { Id = Guid.NewGuid().ToString(), Title = "Notification", Content = "Successfully created room", Type = BlazorNotificationType.Success };
            this.BlazorNotificationService.AddNotification(item);
            StateHasChanged();
        }
        public async Task OpenCreateRoomModal()
        {
            this.InitCreateRoom();
            await this.BlazorModalService.ShowModal(EditRoomModalElementId);
            StateHasChanged();
        }

        public async Task InitEditRoom(int roomId)
        {
            try
            {
                this.roomId = roomId;
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
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }
        public async Task EditRoom()
        {
            try
            {
                ChatHubRoom room = await this.ChatHubService.GetRoom(roomId, ModuleState.ModuleId);
                if (room != null)
                {
                    room.Title = this.title;
                    room.Content = this.content;
                    room.BackgroundColor = this.backgroundcolor;
                    room.Type = this.type;

                    await this.ChatHubService.UpdateRoom(room);
                    await this.CloseModal();
                    this.BlazorAlertsService.NewBlazorAlert("Successfully edited room.", "[Javascript Application]", PositionType.Fixed);
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Saving Room", MessageType.Error);
            }
        }
        public async Task OpenEditRoomModal(int roomId)
        {
            await this.InitEditRoom(roomId);
            await this.BlazorModalService.ShowModal(EditRoomModalElementId);
            StateHasChanged();

            await Task.Delay(1000).ContinueWith(async (task) =>
            {
                await this.GetDefaultDevices();
                this.UpdateUIStateHasChanged();
            });
        }

        public async Task CloseModal()
        {
            await this.BlazorModalService.HideModal(EditRoomModalElementId);
            this.AudioDefaultDevice = null;
            this.MicrophoneDefaultDevice = null;
            this.WebcamDefaultDevice = null;
            this.StateHasChanged();
        }

        public void OnSelect(BlazorSelectEvent e)
        {
            this.type = e.SelectedItem;
            this.UpdateUIStateHasChanged();
        }
        public void BlazorColorPicker_OnSelect(BlazorSelectEvent e)
        {
            this.BlazorColorPickerActiveType = (BlazorColorPickerType)Enum.Parse(typeof(BlazorColorPickerType), e.SelectedItem, true);
            this.UpdateUIStateHasChanged();
        }
        private void OnBlazorColorPickerContextColorChangedExecute(BlazorColorPickerEvent obj)
        {
            this.backgroundcolor = obj.ContextColor;
            this.UpdateUIStateHasChanged();
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
            this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent -= OnBlazorColorPickerContextColorChangedExecute;
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUIStateHasChanged();
        }

    }
}
