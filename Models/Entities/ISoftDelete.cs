namespace BackendAPI.Models.Entities;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}