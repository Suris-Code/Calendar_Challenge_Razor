using MediatR;
using Application.Common.Models;
using Application.Features.Appointments.Contracts;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Appointments.Commands
{
    public record DeleteAppointmentCommand(DeleteAppointmentRequest Request) : IRequest<DeleteAppointmentResponse>;

    public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand, DeleteAppointmentResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteAppointmentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<DeleteAppointmentResponse> Handle(DeleteAppointmentCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
} 