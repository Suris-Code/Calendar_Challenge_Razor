using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Appointments.Contracts
{
    public class CreateAppointmentRequest
    {
        [Required]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        public string? Location { get; set; }
        
        
        public YesNo IsConfirmed { get; set; } = YesNo.No;
        
        public string? MeetingLink { get; set; }
        
        public YesNo SendReminder { get; set; } = YesNo.Yes;
    }
} 