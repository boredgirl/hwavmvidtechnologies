using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.ChatHubs.Models
{
    public class ChatHubCamSequence : ChatHubBaseModel
    {

        public int ChatHubCamId { get; set; }
        public string Filename { get; set; }
        public string FilenameExtension { get; set; }

        [NotMapped] public virtual ChatHubCam Cam { get; set; }
        [NotMapped] public string SequenceBase64DataUri { get; set; }

    }
}
