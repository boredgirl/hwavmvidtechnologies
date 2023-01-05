namespace BlazorPager
{
    public class BlazorPagerEvent<TBlazorPagerItem>
    {

        public int ApiQueryId { get; set; }
        public TBlazorPagerItem Item { get; set; }

    }
}
