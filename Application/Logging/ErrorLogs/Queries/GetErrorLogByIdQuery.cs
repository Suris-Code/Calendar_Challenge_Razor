using Application.Common.Interfaces;
using Application.Logging.ErrorLogs.Dtos;
using AutoMapper;
using MediatR;

namespace Application.Logging.ErrorLogs.Queries;


public class GetErrorLogByIdQuery(int id) : IRequest<ErrorLogDto>
{
    public int Id { get; } = id;
}

public class GetErrorLogByIdQueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper) : IRequestHandler<GetErrorLogByIdQuery, ErrorLogDto>
{
    private readonly IApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<ErrorLogDto> Handle(GetErrorLogByIdQuery request, CancellationToken cancellationToken)
    {
        var logs = await _applicationDbContext.ErrorLogs.FindAsync([request.Id], cancellationToken);
        return _mapper.Map<ErrorLogDto>(logs);
    }

}
