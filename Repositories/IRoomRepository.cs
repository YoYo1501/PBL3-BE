using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAll();
    Task<Room?> GetByIdAsync(int id);
    Task Update(Room room);
}