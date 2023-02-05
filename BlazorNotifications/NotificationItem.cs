namespace BlazorNotifications
{
    public class NotificationItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Timeout { get; set; } = 4200;
        public NotificationType Type { get; set; } = NotificationType.Warning;
    }
}
