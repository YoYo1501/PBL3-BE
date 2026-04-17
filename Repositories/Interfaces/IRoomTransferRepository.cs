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
        Task AddAsync(RoomTransferRequest request);
        Task<List<RoomTransferRequest>> GetAllPendingAsync();
        Task<List<RoomTransferRequest>> GetMyTransfersAsync(int studentId);
        Task UpdateRoomAsync(Room room);
        Task UpdateTransferAsync(RoomTransferRequest request);
        Task SaveChangesAsync();
    }
}
