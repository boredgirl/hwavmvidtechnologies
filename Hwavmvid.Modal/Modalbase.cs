using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hwavmvid.Modal
{
    public class ModalBase : ComponentBase
    {

        [Inject] public Modalservice Modalservice { get; set; }
        [Parameter] public RenderFragment Modalheader { get; set; }
        [Parameter] public RenderFragment Modalbody { get; set; }
        [Parameter] public RenderFragment Modalfooter { get; set; }
        [Parameter] public string ElementId { get; set; }

    }
}
