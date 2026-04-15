using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> GetAll();
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetRoomByStudentIdAsync(int studentId);
    Task AddAsync(Room room);
    Task Update(Room room);
    Task DeleteAsync(Room room);
    Task<bool> BuildingExistsAsync(int buildingId);
}