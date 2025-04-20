namespace HotelApp.DAL.Entities;

public class Booking
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public virtual Room Room { get; set; } // virtual для Lazy Loading

    public string ClientName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
