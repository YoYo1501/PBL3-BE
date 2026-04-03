using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> GetAll();
    Task<Room?> GetByIdAsync(int id);
    Task Update(Room room);
}