using AutoMapper;
using Domain.Entities;
using Application.Features.Appointments.Dtos;
using Application.Features.Appointments.Contracts;

namespace Application.Features.Appointments
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

            // Map request objects to domain entities
            CreateMap<CreateAppointmentRequest, Appointment>();
            CreateMap<UpdateAppointmentRequest, Appointment>();
        }
    }
} 