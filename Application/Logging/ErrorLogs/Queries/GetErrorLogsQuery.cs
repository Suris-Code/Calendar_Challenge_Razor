using Application.Common.Interfaces;
using Application.Logging.ErrorLogs.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Logging.ErrorLogs.Queries;

public class GetErrorLogsQuery : IRequest<List<ErrorLogDto>>
{
}

public class GetErrorLogsQueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper) : IRequestHandler<GetErrorLogsQuery, List<ErrorLogDto>>
{
    private readonly IApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ErrorLogDto>> Handle(GetErrorLogsQuery request, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.ErrorLogs
            .OrderByDescending(c => c.Id)
            .ProjectTo<ErrorLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
