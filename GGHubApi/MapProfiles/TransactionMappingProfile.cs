using AutoMapper;
using GGHubDb.Models;
using GGHubShared.Models;

namespace GGHubApi.MapProfiles
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, TransactionDto>();
        }
    }
}
