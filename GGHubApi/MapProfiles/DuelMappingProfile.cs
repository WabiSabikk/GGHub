using AutoMapper;
using GGHubDb.Models;
using GGHubShared.Models;
using System.Linq;

namespace GGHubApi.MapProfiles
{
    public class DuelMappingProfile : Profile
    {
        public DuelMappingProfile()
        {
            CreateMap<Duel, DuelDto>()
                .ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.Creator))
                .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Winner))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants))
                .ForMember(dest => dest.Maps, opt => opt.MapFrom(src => src.Maps.OrderBy(m => m.Order).Select(m => m.MapName)))
                .ForMember(dest => dest.PrimeOnly, opt => opt.MapFrom(src => src.PrimeOnly))
                .ForMember(dest => dest.WarmupMinutes, opt => opt.MapFrom(src => src.WarmupMinutes))
                .ForMember(dest => dest.CustomMaxRounds, opt => opt.MapFrom(src => src.CustomMaxRounds))
                .ForMember(dest => dest.CustomTickrate, opt => opt.MapFrom(src => src.CustomTickrate))
                .ForMember(dest => dest.CustomOvertimeEnabled, opt => opt.MapFrom(src => src.CustomOvertimeEnabled))
                .ForMember(dest => dest.PreferredRegion, opt => opt.MapFrom(src => src.PreferredRegion))
                .ForMember(dest => dest.ServerConfig, opt => opt.MapFrom(src => src.ServerConfig))
                .ForMember(dest => dest.GameServer, opt => opt.MapFrom(src => src.GameServer));

            CreateMap<DuelParticipant, DuelParticipantDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.IsReady, opt => opt.MapFrom(src => src.IsReady));

            CreateMap<GameServer, GameServerDto>();
        }
    }
}
