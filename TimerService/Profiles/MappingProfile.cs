using AutoMapper;
using TimerService.DataModels;
using Timer = TimerService.Data.Entities.Timer;

namespace TimerService.Profiles
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // TimerRequest -> Timer
            CreateMap<TimerRequest, Timer>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())) // Generate a new GUID for Id
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(_ => DateTime.UtcNow)) // Set DateCreated to current UTC time
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Enums.Status.Started.ToString())); // Set Status to "Started"

            // Timer -> TimerResult
            CreateMap<Timer, TimerResult>();
        }
    }
}
