using Microsoft.AspNetCore.Components;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorColorPicker;

namespace Oqtane.ChatHubs
{
    public class SettingsBase : ModuleBase, IDisposable
    {

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient httpClient { get; set; }
        [Inject] public ISettingService SettingService { get; set; }
        [Inject] public BlazorColorPickerService BlazorColorPickerService { get; set; }

        public string backgroundColor { get; set; }
        public string maxUserNameCharacters { get; set; }
        public string framerate { get; set; }
        public string videoBitsPerSecond { get; set; }
        public string audioBitsPerSecond { get; set; }
        public string videoSegmentsLength { get; set; }
        public string regularExpression { get; set; }

        public List<string> regularExpressions = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent += this.OnColorPickerChangeExecute;

                Dictionary<string, string> settings = await this.SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
                this.backgroundColor = this.SettingService.GetSetting(settings, "BackgroundColor", "#f0f0f0");
                this.maxUserNameCharacters = this.SettingService.GetSetting(settings, "MaxUserNameCharacters", "20");
                this.framerate = this.SettingService.GetSetting(settings, "Framerate", "24");
                this.videoBitsPerSecond = this.SettingService.GetSetting(settings, "VideoBitsPerSecond", "14000");
                this.audioBitsPerSecond = this.SettingService.GetSetting(settings, "AudioBitsPerSecond", "12800");
                this.videoSegmentsLength = this.SettingService.GetSetting(settings, "VideoSegmentsLength", "2400");
                this.regularExpressions = this.SettingService.GetSetting(settings, "RegularExpression", "").Split(";delimiter;", StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }

        public async Task UpdateSettings()
        {
            try
            {
                Dictionary<string, string> settings = await this.SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "BackgroundColor", this.backgroundColor);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "MaxUserNameCharacters", this.maxUserNameCharacters);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "Framerate", this.framerate);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "VideoBitsPerSecond", this.videoBitsPerSecond);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "AudioBitsPerSecond", this.audioBitsPerSecond);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "VideoSegmentsLength", this.videoSegmentsLength);
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);

                this.SettingService.SetSetting(settings, "RegularExpression", string.Join(";delimiter;", regularExpressions));
                await this.SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }

        public void AddRegularExpression_ClickedAsync()
        {
            try
            {
                this.regularExpressions.Add(regularExpression);
                this.regularExpression = string.Empty;
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }

        public void RemoveRegularExpression_ClickedAsync(string item)
        {
            try
            {
                this.regularExpressions.Remove(item);
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }

        private void OnColorPickerChangeExecute(BlazorColorPickerEvent obj)
        {
            this.backgroundColor = obj.ContextColor;
        }

        public void Dispose()
        {
            this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent -= this.OnColorPickerChangeExecute;
        }

    }
}
