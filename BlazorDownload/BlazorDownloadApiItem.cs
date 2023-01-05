namespace BlazorDownload
{
    public class BlazorDownloadApiItem
    {

        public string Id { get; set; }
        public int ProgressTotal { get; set; }
        public int ProgressCurrent { get; set; }
        public string Base64Uri { get; set; }
        public bool DownloadCompleted { get; set; }
        public string Message { get; set; }

    }
}
