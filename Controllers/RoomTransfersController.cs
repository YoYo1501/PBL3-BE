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
            throw new UnauthorizedAccessException("Ng??i d�ng ch?a ??ng nh?p ho?c kh�ng ph?i sinh vi�n");
        }
        return int.Parse(studentIdStr);
    }

    // GET /api/roomtransfers/available � sinh vi�n xem ph�ng tr?ng
    [HttpGet("available")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var studentId = GetStudentId();
        var (success, message, rooms) = await _service.GetAvailableRoomsAsync(studentId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, rooms });
    }
    // GET /api/roomtransfers/my - sinh vien xem lich su yeu cau chuyen phong
    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyTransfers()
    {
        var studentId = GetStudentId();
        var list = await _service.GetMyTransfersAsync(studentId);
        return Ok(list);
    }

    // POST /api/roomtransfers/hold � gi? ch? 10 ph�t
    [HttpPost("hold")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> HoldRoom([FromBody] HoldRoomRequest dto)
    {
        var studentId = GetStudentId();
        var (success, message) = await _service.HoldRoomAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST /api/roomtransfers � x�c nh?n chuy?n ph�ng
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitTransfer([FromBody] RoomTransferRequest dto)
    {
        var studentId = GetStudentId();
        var (success, message) = await _service.SubmitTransferAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET /api/roomtransfers/pending admin xem danh sách yêu cầu chờ duyệt
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPending([FromQuery] RoomTransferPendingQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await _service.GetPagedPendingAsync(query);
            return Ok(paged);
        }

        var list = await _service.GetAllPendingAsync();
        return Ok(list);
    }

    // PUT /api/roomtransfers/{id}/approve admin duyệt hoặc từ chối yêu cầu chuyển phòng
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveTransfer dto)
    {
        var (success, message) = await _service.ApproveTransferAsync(id, dto.IsApproved, dto.RejectionReason);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}

