namespace HotelApp.WebAPI.Models;

public class BookingDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}