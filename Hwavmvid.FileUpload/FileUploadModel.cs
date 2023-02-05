using Microsoft.AspNetCore.Components.Forms;

namespace Hwavmvid.FileUpload
{
    public class FileUploadModel
    {

        public string Base64ImageUrl { get; set; }

        public IBrowserFile BrowserFile { get; set; }

    }
}
