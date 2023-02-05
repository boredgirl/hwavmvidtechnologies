namespace Hwavmvid.Pager
{
    public class PagerEvent<TPagerItem>
    {

        public int ApiQueryId { get; set; }
        public TPagerItem Item { get; set; }

    }
}
