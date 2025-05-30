using AutoMapper;
using Domain.Entities;
using Application.Logging.EmailLog.Dtos;

namespace Application.Common.Mappings.Profiles;

public class EmailLogProfile : Profile
{
    public EmailLogProfile()
    {
        CreateMap<Domain.Entities.EmailLog, EmailLogDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ToAddresses, opt => opt.MapFrom(src => src.ToAddresses))
            .ForMember(dest => dest.FromName, opt => opt.MapFrom(src => src.FromName))
            .ForMember(dest => dest.FromAddress, opt => opt.MapFrom(src => src.FromAddress))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Body))
            .ForMember(dest => dest.SentDate, opt => opt.MapFrom(src => src.SentDate))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullUserName, opt => opt.MapFrom(src => src.FullUserName))
            .ForMember(dest => dest.IsBodyHtml, opt => opt.MapFrom(src => src.IsBodyHtml))
            .ForMember(dest => dest.ResentEmail, opt => opt.MapFrom(src => src.ResentEmail));
    }
} 