using BackendAPI.Models.DTOs.Room;
using BackendAPI.Models.DTOs.RoomTransfer;
using BackendAPI.Models.DTOs.RoomTransfer.Requests;

namespace BackendAPI.Services;

public interface IRoomTransferService
{
    Task<(bool Success, string Message, List<RoomDto>? Rooms)> GetAvailableRoomsAsync(int studentId);
    Task<(bool Success, string Message)> HoldRoomAsync(int studentId, HoldRoomRequest dto);

    Task<(bool Success, string Message)> SubmitTransferAsync(int studentId, RoomTransferRequest dto);
    Task<(bool Success, string Message)> ApproveTransferAsync(int requestId, bool isApproved, string? rejectionReason);
    Task<List<RoomTransferResponseDto>> GetAllPendingAsync();
    Task<List<RoomTransferResponseDto>> GetMyTransfersAsync(int studentId);
}