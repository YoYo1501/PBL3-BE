using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IFacilityRepository
{
    Task<Facility?> GetByIdAsync(int id);
    Task<List<Facility>> GetAllAsync();
    Task<List<Facility>> GetByRoomIdAsync(int roomId);
    Task AddAsync(Facility facility);
    void Update(Facility facility);
    void Delete(Facility facility);
    Task SaveChangesAsync();
}