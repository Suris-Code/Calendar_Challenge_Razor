using AutoMapper;
using Domain.Entities;
using Application.Logging.ActivityLog.Dtos;

namespace Application.Common.Mappings.Profiles;

public class ActivityLogProfile : Profile
{
    public ActivityLogProfile()
    {
        CreateMap<Domain.Entities.ActivityLog, ActivityLogDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullUserName, opt => opt.MapFrom(src => src.FullUserName))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.ControllerName, opt => opt.MapFrom(src => src.ControllerName))
            .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.ActionName))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName));
    }
} 