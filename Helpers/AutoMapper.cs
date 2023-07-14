using AutoMapper;
using LibraryAPI.Models;
using LibraryAPI.Models.Dtos;

namespace LibraryAPI.Helpers
{
    public class AutoMapper: Profile
    {
        public AutoMapper()
        {
            CreateMap<UserDTO, User>();
            CreateMap<BookDTO, Book>();
            CreateMap<OrderDTO, Order>();
        }
    }
}
