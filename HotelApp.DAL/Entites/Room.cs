namespace HotelApp.DAL.Entities;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public RoomStatus Status { get; set; }

    public int CategoryId { get; set; }
    public virtual RoomCategory Category { get; set; } // virtual для Lazy Loading

    public virtual ICollection<Booking> Bookings { get; set; } // virtual для Lazy Loading
}
