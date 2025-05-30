﻿namespace Domain.Entities
{
    public class EmailLog
    {
        public long Id { get; set; }
        public string ToAddresses { get; set; } 
        public string? BccAddresses { get; set; }
        public string FromName { get; set; } 
        public string FromAddress { get; set; } 
        public string Subject { get; set; } 
        public string Body { get; set; } 
        public DateTime SentDate { get; set; }
        public string? UserId { get; set; } 
        public string? FullUserName { get; set; }
        public bool IsBodyHtml { get; set; }
        public bool ResentEmail { get; set; }
        public string? ErrorMessage { get; set; }

        public ApplicationUser? User { get; set; }

    }
}
