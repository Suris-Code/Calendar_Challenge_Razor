namespace Application.Logging.ActivityLog.Dtos
{
    public class ActivityLogDto
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string FullUserName { get; set; }
        public string Area { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string DisplayName { get; set; }
    }
}