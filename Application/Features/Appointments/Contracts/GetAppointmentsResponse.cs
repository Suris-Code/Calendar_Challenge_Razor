using Application.Common.Models;
using Application.Features.Appointments.Dtos;
using System.Collections.Generic;

namespace Application.Features.Appointments.Contracts
{
    public class GetAppointmentsResponse : PaginatedResponse<IEnumerable<AppointmentDto>>
    {
    }
} 