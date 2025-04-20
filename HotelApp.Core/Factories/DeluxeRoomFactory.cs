using HotelApp.DAL.Entities;

namespace HotelApp.Core.Factories;

public class DeluxeRoomFactory : IRoomFactory
{
    public (Room Room, RoomCategory Category) CreateRoom(string number)
    {
        var category = new RoomCategory
        {
            Name = "Делюкс",
            BasePrice = 2000
        };

        var room = new Room
        {
            Number = number,
            Category = category,
            Status = RoomStatus.Available
        };

        return (room, category);
    }
}