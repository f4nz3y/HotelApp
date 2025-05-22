namespace HotelApp.WebAPI.Models.Requests;

public class CreateRoomRequest
{
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = "standard"; // "standard" або "deluxe"
}