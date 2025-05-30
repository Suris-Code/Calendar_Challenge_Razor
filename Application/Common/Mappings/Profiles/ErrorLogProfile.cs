using Application.Logging.ErrorLogs.Dtos;
using AutoMapper;
using Domain.Entities;
namespace Application.Common.Mappings.Profiles
{
    public class ErrorLogProfile : Profile
    {
        public ErrorLogProfile()
        {
            CreateMap<Domain.Entities.ErrorLog, ErrorLogDto>();
        }
    }
} 