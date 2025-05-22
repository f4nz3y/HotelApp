using AutoMapper;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Models;
using HotelApp.WebAPI.Models;
using HotelApp.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IMapper _mapper;

    public BookingsController(IRoomService roomService, IMapper mapper)
    {
        _roomService = roomService;
        _mapper = mapper;
    }

    [HttpPost("preview")]
    public ActionResult<BookingDto> PreviewBooking([FromBody] CreateBookingRequest request)
    {
        var (room, totalPrice, booking) = _roomService.PreviewBooking(
            request.RoomId,
            request.StartDate,
            request.EndDate,
            request.ClientName
        );

        if (booking == null)
            return BadRequest("Бронювання неможливе для вказаних параметрів");

        var bookingDto = _mapper.Map<BookingDto>(booking);
        bookingDto.TotalPrice = totalPrice;

        return Ok(bookingDto);
    }

    [HttpPost]
    public ActionResult<BookingDto> ConfirmBooking([FromBody] CreateBookingRequest request)
    {
        var (_, _, booking) = _roomService.PreviewBooking(
            request.RoomId,
            request.StartDate,
            request.EndDate,
            request.ClientName
        );

        if (booking == null)
            return BadRequest("Неможливо створити бронювання");

        _roomService.ConfirmBooking(booking);
        return Ok(_mapper.Map<BookingDto>(booking));
    }

    [HttpDelete("{roomId}")]
    public IActionResult CancelBooking(int roomId)
    {
        var room = _roomService.GetRoom(roomId);
        if (room == null) return NotFound();

        _roomService.CancelBooking(roomId);
        return NoContent();
    }
}