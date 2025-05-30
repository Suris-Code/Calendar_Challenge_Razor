using AutoMapper;
using Domain.Entities;
using Application.Logging.UserActivityLog.Dtos;

namespace Application.Common.Mappings.Profiles;

public class UserActivityLogProfile : Profile
{
    public UserActivityLogProfile()
    {
        CreateMap<Domain.Entities.UserActivityLog, UserActivityLogDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress));
    }
} 