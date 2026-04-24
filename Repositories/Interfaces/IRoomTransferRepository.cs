using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces
{
    public interface IRoomTransferRepository
    {
        Task<Contract?> GetActiveContractAsync(int studentId);
        Task<int> CountTransferInSemesterAsync(int studentId, int semesterId);
        Task<SemesterPeriods?> GetCurrentSemesterAsync();
        Task<List<Room>> GetAvailableRoomsAsync(string gender, int excludeRoomId);
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<RoomTransferRequest?> GetPendingTransferAsync(int studentId);
        Task<RoomTransferRequest?> GetPendingTransferByIdAsync(int requestId);
        Task<RoomTransferRequest?> GetTransferByIdAsync(int requestId);
        Task AddAsync(RoomTransferRequest request);
        Task<List<RoomTransferRequest>> GetAllPendingAsync();
        Task<(List<RoomTransferRequest> Items, int TotalCount)> GetPagedPendingAsync(int page, int pageSize);
        Task<List<RoomTransferRequest>> GetMyTransfersAsync(int studentId);
        Task UpdateContractAsync(Contract contract);
        Task UpdateRoomAsync(Room room);
        Task UpdateTransferAsync(RoomTransferRequest request);
        Task SaveChangesAsync();
    }
}
