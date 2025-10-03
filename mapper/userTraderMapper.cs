using AutoMapper;
using TradeSphere3.Models;
using TradeSphere3.DTOs;

namespace TradeSphere3.Mapper
{
    public class UserTraderMapper : Profile
{
        public UserTraderMapper()
        {
            // User ↔ DTO
            CreateMap<ApplicationUser, UserWithTraderDto>().ReverseMap();

            // Trader ↔ DTO
            CreateMap<Trader, TraderDto>().ReverseMap();
        }
    }
}