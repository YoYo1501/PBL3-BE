using BackendAPI.Models.DTOs.RoomTransfer;
using BackendAPI.Models.DTOs.RoomTransfer.Requests;
using BackendAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomTransfersController(IRoomTransferService _service) : ControllerBase
{
    private int GetStudentId()
    {
        var studentIdStr = User.FindFirstValue("StudentId");
        if (string.IsNullOrEmpty(studentIdStr))
        {
            throw new UnauthorizedAccessException("Ng??i dùng ch?a ??ng nh?p ho?c không ph?i sinh viên");
        }
        return int.Parse(studentIdStr);
    }

    // GET /api/roomtransfers/available — sinh viên xem phòng tr?ng
    [HttpGet("available")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var studentId = GetStudentId();
        var (success, message, rooms) = await _service.GetAvailableRoomsAsync(studentId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, rooms });
    }

    // POST /api/roomtransfers/hold — gi? ch? 10 phút
    [HttpPost("hold")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> HoldRoom([FromBody] HoldRoomRequest dto)
    {
        var studentId = GetStudentId();
        var (success, message) = await _service.HoldRoomAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST /api/roomtransfers — xác nh?n chuy?n phòng
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitTransfer([FromBody] RoomTransferRequest dto)
    {
        var studentId = GetStudentId();
        var (success, message) = await _service.SubmitTransferAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET /api/roomtransfers/pending — admin xem danh sách ch? duy?t
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPending()
    {
        var list = await _service.GetAllPendingAsync();
        return Ok(list);
    }

    // PUT /api/roomtransfers/{id}/approve — admin duy?t/t?i
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveTransfer dto)
    {
        var (success, message) = await _service.ApproveTransferAsync(id, dto.IsApproved, dto.RejectionReason);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
