namespace HotelApp.WebAPI.Models.Requests;

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}