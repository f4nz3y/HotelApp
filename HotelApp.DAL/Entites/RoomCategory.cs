namespace HotelApp.DAL.Entities;

public class RoomCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal BasePrice { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } // virtual для Lazy Loading
}
