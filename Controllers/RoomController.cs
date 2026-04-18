using BackendAPI.Models.DTOs.Room;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController(IRoomService roomService) : ControllerBase
{
    private int GetStudentId()
    {
        var studentIdStr = User.FindFirstValue("StudentId");
        if (string.IsNullOrEmpty(studentIdStr))
        {
            throw new UnauthorizedAccessException("Nguoi dung chua dang nhap hoac khong phai sinh vien");
        }
        return int.Parse(studentIdStr);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllRooms([FromQuery] RoomListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize") ||
            Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("status"))
        {
            var paged = await roomService.GetPagedRoomsAsync(query);
            return Ok(paged);
        }

        var rooms = await roomService.GetAllRooms();
        return Ok(rooms);
    }

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var rooms = await roomService.GetAvailableRooms();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var room = await roomService.GetRoomByIdAsync(id);
        if (room == null) return NotFound(new { message = "Phong khong ton tai" });
        return Ok(room);
    }

    [HttpGet("my-room")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyRoom()
    {
        var studentId = GetStudentId();
        var room = await roomService.GetMyRoomAsync(studentId);

        if (room == null)
            return NotFound(new { message = "Ban hien khong co phong nao dang co hop dong luu tru (Active)." });

        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
    {
        var (success, message, data) = await roomService.CreateRoomAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto dto)
    {
        var (success, message) = await roomService.UpdateRoomAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var (success, message) = await roomService.DeleteRoomAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
