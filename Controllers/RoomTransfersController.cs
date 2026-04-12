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
    private int GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập");
        }
        return int.Parse(userIdStr);
    }

    // GET /api/roomtransfers/available — sinh viên xem phòng trống
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var studentId = GetUserId();
        var (success, message, rooms) = await _service.GetAvailableRoomsAsync(studentId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, rooms });
    }

    // POST /api/roomtransfers/hold — giữ chỗ 10 phút
    [HttpPost("hold")]
    public async Task<IActionResult> HoldRoom([FromBody] HoldRoomRequest dto)
    {
        var studentId = GetUserId();
        var (success, message) = await _service.HoldRoomAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST /api/roomtransfers — xác nhận chuyển phòng
    [HttpPost]
    public async Task<IActionResult> SubmitTransfer([FromBody] RoomTransferRequest dto)
    {
        var studentId = GetUserId();
        var (success, message) = await _service.SubmitTransferAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET /api/roomtransfers/pending — admin xem danh sách chờ duyệt
    [HttpGet("pending")]
    public async Task<IActionResult> GetAllPending()
    {
        var list = await _service.GetAllPendingAsync();
        return Ok(list);
    }

    // PUT /api/roomtransfers/{id}/approve — admin duyệt/từ chối
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveTransfer dto)
    {
        var (success, message) = await _service.ApproveTransferAsync(id, dto.IsApproved, dto.RejectionReason);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}