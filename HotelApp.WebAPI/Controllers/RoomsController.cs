using AutoMapper;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Models;
using HotelApp.WebAPI.Models;
using HotelApp.WebAPI.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IMapper _mapper;

    public RoomsController(IRoomService roomService, IMapper mapper)
    {
        _roomService = roomService;
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<RoomDto>> GetAllRooms()
    {
        var rooms = _roomService.GetAllRooms();
        return Ok(_mapper.Map<IEnumerable<RoomDto>>(rooms));
    }

    [HttpGet("available")]
    public ActionResult<IEnumerable<RoomDto>> GetAvailableRooms([FromQuery] DateTime date)
    {
        var rooms = _roomService.GetAvailableRooms(date);
        return Ok(_mapper.Map<IEnumerable<RoomDto>>(rooms));
    }

    [HttpGet("{id}")]
    public ActionResult<RoomDto> GetRoom(int id)
    {
        var room = _roomService.GetRoom(id);
        if (room == null) return NotFound();
        return Ok(_mapper.Map<RoomDto>(room));
    }

    [HttpPost]
    public ActionResult<RoomDto> CreateRoom([FromBody] CreateRoomRequest request)
    {
        _roomService.AddRoomViaFactory(request.Type, request.Number);
        var room = _roomService.GetAllRooms().LastOrDefault();
        if (room == null) return BadRequest();

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id },
            _mapper.Map<RoomDto>(room));
    }

    [HttpPut("{id}")]
    public IActionResult UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
    {
        try
        {
            if (id != request.Id)
                return BadRequest("Id in URL doesn't match Id in request body");

            var roomModel = new RoomModel
            {
                Id = id,
                Number = request.Number,
                CategoryId = request.CategoryId,
                Status = request.Status
            };

            _roomService.UpdateRoom(roomModel);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteRoom(int id)
    {
        var room = _roomService.GetRoom(id);
        if (room == null) return NotFound();

        _roomService.DeleteRoom(id);
        return NoContent();
    }
}