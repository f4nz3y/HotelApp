using HotelApp.DAL.Entities;

namespace HotelApp.Core.Factories;

public interface IRoomFactory
{
    (Room Room, RoomCategory Category) CreateRoom(string number);
}