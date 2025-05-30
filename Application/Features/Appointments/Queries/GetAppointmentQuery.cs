using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Models;
using Domain.Entities;
using Application.Features.Appointments.Contracts;
using Application.Features.Appointments.Dtos;
using Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Appointments.Queries
{
    public record GetAppointmentQuery(GetAppointmentRequest Request) : IRequest<GetAppointmentResponse>;

    public class GetAppointmentQueryHandler : IRequestHandler<GetAppointmentQuery, GetAppointmentResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetAppointmentQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<GetAppointmentResponse> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
        {
            var isAdmin = _currentUserService.Claims.Any(c => c == "Admin");
            
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == request.Request.Id && 
                    (isAdmin || a.UserId == _currentUserService.UserId), cancellationToken);

            if (appointment == null)
            {
                return new GetAppointmentResponse 
                { 
                    Result = Result.Failure("Appointment not found or you don't have permission to view it.") 
                };
            }

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);

            return new GetAppointmentResponse
            {
                Data = appointmentDto,
                Result = Result.Success()
            };
        }
    }
} 