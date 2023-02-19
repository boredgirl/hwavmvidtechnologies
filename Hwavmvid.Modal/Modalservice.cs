using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Hwavmvid.Modal
{

    public class Modalservice
    {

        public IJSObjectReference Module { get; set; }
        public IJSRuntime Jsruntime { get; set; }
        public IJSObjectReference Jsobjref { get; set; }

        public DotNetObjectReference<Modalserviceextension> DotNetObjectRef;
        public Modalserviceextension Modalserviceextension;

        public event Action RunUpdateUI;

        public Modalservice(IJSRuntime jsRuntime)
        {
            this.Jsruntime = jsRuntime;
            this.Modalserviceextension = new Modalserviceextension(this);
            this.DotNetObjectRef = DotNetObjectReference.Create(this.Modalserviceextension);
        }
        public async Task Initmodal()
        {
            if(this.Module == null || this.Jsobjref == null)
            {
                this.Module = await this.Jsruntime.InvokeAsync<IJSObjectReference>("import", "/Modules/Oqtane.ChatHubs/modaljsinterop.js");
                this.Jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initmodal", this.DotNetObjectRef);
            }
        }

        public async Task ShowModal(string id)
        {
            await this.Jsobjref.InvokeVoidAsync("showmodal", id);
            this.RunUpdateUI?.Invoke();
        }

        public async Task HideModal(string id)
        {
            await this.Jsobjref.InvokeVoidAsync("hidemodal", id);
            this.RunUpdateUI?.Invoke();
        }

    }

    public class Modalserviceextension
    {

        public Modalservice Modalservice { get; set; }
        public event Action<string> OnModalShown;
        public event Action<string> OnModalHidden;

        public Modalserviceextension(Modalservice modalservice)
        {
            this.Modalservice = modalservice;
        }

        [JSInvokable("ModalShown")]
        public void ModalShown(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                this.OnModalShown?.Invoke(id);
            }
        }

        [JSInvokable("ModalHidden")]
        public void ModalHidden(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                this.OnModalHidden?.Invoke(id);
            }
        }

    }

}
