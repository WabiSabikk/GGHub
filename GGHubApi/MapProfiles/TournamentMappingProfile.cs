using AutoMapper;
using GGHubDb.Models;
using GGHubShared.Models;

namespace GGHubApi.MapProfiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Tournament, TournamentDto>()
                .ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.Creator))
                .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Winner))
                .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.Teams))
                .ForMember(dest => dest.Matches, opt => opt.MapFrom(src => src.Matches))
                .ForMember(dest => dest.Maps, opt => opt.MapFrom(src => src.Maps.OrderBy(m => m.Order).Select(m => m.MapName)));

            CreateMap<TournamentTeam, TournamentTeamDto>()
                .ForMember(dest => dest.Captain, opt => opt.MapFrom(src => src.Captain))
                .ForMember(dest => dest.Players, opt => opt.MapFrom(src => src.Players));

            CreateMap<TournamentPlayer, TournamentPlayerDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<TournamentMatch, TournamentMatchDto>()
                .ForMember(dest => dest.Team1, opt => opt.MapFrom(src => src.Team1))
                .ForMember(dest => dest.Team2, opt => opt.MapFrom(src => src.Team2))
                .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Winner));

            CreateMap<TournamentPayment, TournamentPaymentDto>()
                .ForMember(dest => dest.Team, opt => opt.MapFrom(src => src.Team))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<User, UserDto>();
        }
    }
}
