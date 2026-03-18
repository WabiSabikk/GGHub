using AutoMapper;
using DatHost.Models;
using GGHubDb.Models;
using GGHubShared.Models;

namespace GGHubApi.MapProfiles;

public class ServerMappingProfile : Profile
{
    public ServerMappingProfile()
    {
        CreateMap<GameServerResponse, DathostServerResult>()
            .ForMember(d => d.Success, opt => opt.MapFrom(_ => true))
            .ForMember(d => d.ServerId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.ServerIp, opt => opt.MapFrom(s => s.Ip))
            .ForMember(d => d.ServerPort, opt => opt.MapFrom(s => s.Ports.Game))
            .ForMember(d => d.Password, opt => opt.MapFrom(s => GetCs2Rcon(s)))
            .ForMember(d => d.Rcon, opt => opt.MapFrom(s => GetCs2Rcon(s)))
            .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
            .ForMember(d => d.Slots, opt => opt.MapFrom(s => GetCs2Slots(s)))
            .ForMember(d => d.Tickrate, opt => opt.MapFrom(s => GetCs2Tickrate(s)))
            .ForMember(d => d.RawIp, opt => opt.MapFrom(s => s.RawIp))
            .ForMember(d => d.Autostop, opt => opt.MapFrom(s => s.Autostop))
            .ForMember(d => d.AutostopMinutes, opt => opt.MapFrom(s => s.AutostopMinutes))
            .ForMember(d => d.On, opt => opt.MapFrom(s => s.On))
            .ForMember(d => d.Booting, opt => opt.MapFrom(s => s.Booting))
            .ForMember(d => d.PlayersOnline, opt => opt.MapFrom(s => s.PlayersOnline))
            .ForMember(d => d.ServerError, opt => opt.MapFrom(s => s.ServerError))
            .ForMember(d => d.Confirmed, opt => opt.MapFrom(s => s.Confirmed))
            .ForMember(d => d.CostPerHour, opt => opt.MapFrom(s => s.CostPerHour))
            .ForMember(d => d.MaxCostPerHour, opt => opt.MapFrom(s => s.MaxCostPerHour))
            .ForMember(d => d.MonthResetAt, opt => opt.MapFrom(s => s.MonthResetAtDateTime))
            .ForMember(d => d.MaxCostPerMonth, opt => opt.MapFrom(s => s.MaxCostPerMonth))
            .ForMember(d => d.ManualSortOrder, opt => opt.MapFrom(s => s.ManualSortOrder))
            .ForMember(d => d.DiskUsageBytes, opt => opt.MapFrom(s => s.DiskUsageBytes))
            .ForMember(d => d.DeletionProtection, opt => opt.MapFrom(s => s.DeletionProtection))
            .ForMember(d => d.OngoingMaintenance, opt => opt.MapFrom(s => s.OngoingMaintenance))
            .ForMember(d => d.ConnectString, opt => opt.MapFrom<ConnectStringResolver>())
            .ForMember(d => d.SteamConnectUrl, opt => opt.MapFrom<SteamUrlResolver>())
            .ForMember(d => d.MatchId, opt => opt.MapFrom(s => s.MatchId))
            .ForMember(d => d.ErrorMessage, opt => opt.Ignore());

        CreateMap<DathostServerResult, GameServer>()
            .ForMember(g => g.ExternalServerId, opt => opt.MapFrom(d => d.ServerId))
            .ForMember(g => g.ServerIp, opt => opt.MapFrom(d => d.ServerIp))
            .ForMember(g => g.ServerPort, opt => opt.MapFrom(d => d.ServerPort))
            .ForMember(g => g.Password, opt => opt.MapFrom(d => d.Password))
            .ForMember(g => g.Rcon, opt => opt.MapFrom(d => d.Rcon))
            .ForMember(g => g.RawIp, opt => opt.MapFrom(d => d.RawIp))
            .ForMember(g => g.Location, opt => opt.MapFrom(d => d.Location))
            .ForMember(g => g.Slots, opt => opt.MapFrom(d => d.Slots ?? 0))
            .ForMember(g => g.Autostop, opt => opt.MapFrom(d => d.Autostop))
            .ForMember(g => g.AutostopMinutes, opt => opt.MapFrom(d => d.AutostopMinutes))
            .ForMember(g => g.On, opt => opt.MapFrom(d => d.On))
            .ForMember(g => g.Booting, opt => opt.MapFrom(d => d.Booting))
            .ForMember(g => g.PlayersOnline, opt => opt.MapFrom(d => d.PlayersOnline))
            .ForMember(g => g.ServerError, opt => opt.MapFrom(d => d.ServerError))
            .ForMember(g => g.Confirmed, opt => opt.MapFrom(d => d.Confirmed))
            .ForMember(g => g.CostPerHour, opt => opt.MapFrom(d => d.CostPerHour))
            .ForMember(g => g.MaxCostPerHour, opt => opt.MapFrom(d => d.MaxCostPerHour))
            .ForMember(g => g.MonthResetAt, opt => opt.MapFrom(d => d.MonthResetAt))
            .ForMember(g => g.MaxCostPerMonth, opt => opt.MapFrom(d => d.MaxCostPerMonth))
            .ForMember(g => g.ManualSortOrder, opt => opt.MapFrom(d => d.ManualSortOrder))
            .ForMember(g => g.DiskUsageBytes, opt => opt.MapFrom(d => d.DiskUsageBytes))
            .ForMember(g => g.DeletionProtection, opt => opt.MapFrom(d => d.DeletionProtection))
            .ForMember(g => g.OngoingMaintenance, opt => opt.MapFrom(d => d.OngoingMaintenance));
    }

    private static string? GetCs2Rcon(GameServerResponse source)
        => source.Cs2Settings?.Rcon;

    private static int? GetCs2Slots(GameServerResponse source)
        => source.Cs2Settings?.Slots;

    private static int? GetCs2Tickrate(GameServerResponse source)
        => source.Cs2Settings?.Tickrate;
}

public class ConnectStringResolver : IValueResolver<GameServerResponse, DathostServerResult, string>
{
    public string Resolve(GameServerResponse source, DathostServerResult destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.Ip))
            return string.Empty;

        return $"connect {source.Ip}:{source.Ports?.Game}";
    }
}

public class SteamUrlResolver : IValueResolver<GameServerResponse, DathostServerResult, string>
{
    public string Resolve(GameServerResponse source, DathostServerResult destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.Ip) || source.Ports?.Game == null)
            return string.Empty;

        var gameServerDto = new GameServerDto
        {
            ServerIp = source.Ip,
            ServerPort = source.Ports.Game,
            Password = source.Cs2Settings?.Rcon
        };

        return gameServerDto.SteamUrl ?? string.Empty;
    }
}