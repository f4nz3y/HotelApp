using System;

namespace HotelApp.Core.Models
{
    public class BookingModel
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}