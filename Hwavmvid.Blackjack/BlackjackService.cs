using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Hwavmvid.Blackjack
{

    public class BlackjackService : IDisposable
    {

        private IJSObjectReference javascriptfile;
        private IJSRuntime jsruntime;

        public BlackjackService(IJSRuntime jsRuntime)
        {
            this.jsruntime = jsRuntime;
        }

        public async Task InitBlackjackService()
        {
            this.javascriptfile = await this.jsruntime.InvokeAsync<IJSObjectReference>(
               "import", "/Modules/Oqtane.ChatHubs/blackjackjsinterop.js");
        }

        public async Task<string> Prompt(string message)
        {
            return await this.javascriptfile.InvokeAsync<string>("showPrompt", message);
        }

        public void Dispose()
        {
            if (javascriptfile != null)
                this.javascriptfile.DisposeAsync();
        }

    }
}