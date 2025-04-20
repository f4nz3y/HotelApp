namespace HotelApp.DAL.Entities;

public class RoomCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal BasePrice { get; set; }

    public ICollection<Room> Rooms { get; set; }
}