using Domain.Enums;
using System;

namespace Application.Features.Appointments.Dtos
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public YesNo IsConfirmed { get; set; }
        public YesNo IsCancelled { get; set; }
        public string CancellationReason { get; set; }
        public string MeetingLink { get; set; }
        public YesNo SendReminder { get; set; }
        public DateTime? ReminderSentAt { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    }
} 