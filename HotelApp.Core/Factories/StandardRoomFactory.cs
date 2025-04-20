using HotelApp.DAL.Entities;

namespace HotelApp.Core.Factories;

public class StandardRoomFactory : IRoomFactory
{
    public (Room Room, RoomCategory Category) CreateRoom(string number)
    {
        var category = new RoomCategory
        {
            Name = "Стандарт",
            BasePrice = 1000
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