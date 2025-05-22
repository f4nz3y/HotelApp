namespace HotelApp.WebAPI.Models.Requests;

public class UpdateRoomRequest
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
}