using Domain.Common;

namespace Domain.Entities
{
    public class UserActivityLog
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
