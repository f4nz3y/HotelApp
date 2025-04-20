namespace HotelApp.DAL.Entities;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public RoomStatus Status { get; set; }

    public int CategoryId { get; set; }
    public RoomCategory Category { get; set; }

    public ICollection<Booking> Bookings { get; set; }
}