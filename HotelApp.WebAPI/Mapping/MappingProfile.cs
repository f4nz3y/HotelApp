using AutoMapper;
using HotelApp.Core.Models;
using HotelApp.DAL.Entities;
using HotelApp.WebAPI.Models;
using HotelApp.WebAPI.Models.Requests;

namespace HotelApp.WebAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity -> Model
        CreateMap<Room, RoomModel>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryBasePrice,
                opt => opt.MapFrom(src => src.Category.BasePrice))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<RoomModel, Room>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => Enum.Parse<RoomStatus>(src.Status)));

        CreateMap<Booking, BookingModel>();
        CreateMap<BookingModel, Booking>();

        // DTO <-> Model
        CreateMap<RoomModel, RoomDto>()
            .ForMember(dest => dest.BasePrice,
                opt => opt.MapFrom(src => src.CategoryBasePrice));
        CreateMap<RoomDto, RoomModel>();

        CreateMap<BookingModel, BookingDto>();
        CreateMap<BookingDto, BookingModel>();

        // Request -> Model
        CreateMap<CreateBookingRequest, BookingModel>();
        CreateMap<UpdateRoomRequest, RoomModel>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CategoryId,
                opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Number,
                opt => opt.MapFrom(src => src.Number));
    }
}