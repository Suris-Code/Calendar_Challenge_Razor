namespace Application.Logging.UserActivityLog.Dtos
{
    public class UserActivityLogDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }
    }
}