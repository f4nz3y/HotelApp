namespace HotelApp.Core.Models
{
    public class RoomModel
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal CategoryBasePrice { get; set; }
        public string Status { get; set; }
    }
}