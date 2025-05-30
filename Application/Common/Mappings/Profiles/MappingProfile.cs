using Application.Features.Appointments.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty));

        }
    }
} 