using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Logging.ErrorLogs.Contracts;
using Application.Logging.ErrorLogs.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Logging.ErrorLogs.Commands
{
    public class CreateErrorLogCommand(CreateErrorLogRequest request) : IRequest<CreateErrorLogResponse>
    {
        readonly CreateErrorLogRequest _request = request;

        public CreateErrorLogRequest Request => _request;
    }

    public class CreateErrorLogCommandHandler(
        IApplicationDbContext applicationDbContext,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
        ) : IRequestHandler<CreateErrorLogCommand, CreateErrorLogResponse>
    {
        private readonly IApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper _mapper = mapper;

        public async Task<CreateErrorLogResponse> Handle(CreateErrorLogCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.Request.Message))
                {
                    throw new NotImplementedException("Error message could not be empty");
                }

                var error = new Domain.Entities.ErrorLog
                {
                    Message = command.Request.Message,
                    Type = command.Request.Type,
                    HttpErrorCode = command.Request.HttpErrorCode,
                    Path = command.Request.Path,
                    QueryString = command.Request.QueryString,
                    MemberName = command.Request.MemberName,
                    SourceFilePath = command.Request.Path,
                    SourceLineNumber = command.Request.SourceLineNumber,
                    SourceColumnNumber = command.Request.SourceColumnNumber
                };

                if (_currentUserService.UserId != null)
                {
                    error.UserId = _currentUserService.UserId;
                    error.FullUserName = String.Format("{0} {1}", _currentUserService.FirstName, _currentUserService.LastName);
                }
                else
                {
                    error.UserId = "Anonymous";
                    error.FullUserName = "";
                }
                error.RemoteIPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                error.RemotePort = _httpContextAccessor.HttpContext.Connection.RemotePort.ToString();
                error.LocalIPAddress = _httpContextAccessor.HttpContext.Connection.LocalIpAddress.ToString();
                error.LocalPort = _httpContextAccessor.HttpContext.Connection.LocalPort.ToString();
                error.Date = DateTime.Now;

                await _applicationDbContext.ErrorLogs.AddAsync(error, cancellationToken);

                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                var dto = _mapper.Map<ErrorLogDto>(error);

                return new CreateErrorLogResponse()
                {
                    Result = Result.Success(),
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new CreateErrorLogResponse()
                {
                    Result = Result.Failure(ex.Message)
                };
            }
        }
    }

}