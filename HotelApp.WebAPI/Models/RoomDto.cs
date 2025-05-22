namespace HotelApp.WebAPI.Models;

public class RoomDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; } 
    public string Status { get; set; } = string.Empty;
}