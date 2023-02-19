using Microsoft.AspNetCore.Components;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using Hwavmvid.Modal;

namespace Oqtane.ChatHubs
{
    public class SettingsModalBase : ModuleBase
    {

        [Inject] public ChatHubService ChatHubService { get; set; }
        [Inject] public Modalservice ModalService { get; set; }

        public const string SettingsModalElementId = "SettingsModalElementId";

        public async void OpenDialogAsync()
        {
            await this.ModalService.ShowModal(SettingsModalElementId);
            StateHasChanged();
        }

        public async void CloseDialogAsync()
        {
            await this.ModalService.HideModal(SettingsModalElementId);
            StateHasChanged();
        }

    }
}
