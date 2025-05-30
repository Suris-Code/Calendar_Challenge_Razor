using Domain.Common;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Appointment : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        public string? Location { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public virtual ApplicationUser? User { get; set; }
        
        [Required]
        public YesNo IsConfirmed { get; set; } = YesNo.No;
        
        [Required]
        public YesNo IsCancelled { get; set; } = YesNo.No;
        
        public string? CancellationReason { get; set; }
        
        public string? MeetingLink { get; set; }
        
        [Required]
        public YesNo SendReminder { get; set; } = YesNo.Yes;
        
        public DateTime? ReminderSentAt { get; set; }
    }
} 