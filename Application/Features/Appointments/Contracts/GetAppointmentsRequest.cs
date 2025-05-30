using Application.Common.Models;
using System;

namespace Application.Features.Appointments.Contracts
{
    public class GetAppointmentsRequest : PaginatedWithFilter<GetAppointmentsFilters>
    {
    }

    public class GetAppointmentsFilters
    {
        public DatetimeRange? AppointmentDateRange { get; set; }
        public string UserId { get; set; }
        public bool? ShowOnlyConfirmed { get; set; }
        public bool? ShowOnlyCancelled { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? WeekStart { get; set; }
    }
} 