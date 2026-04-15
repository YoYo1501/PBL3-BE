using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.DTOs.Room
{
    public class CreateRoomDto
    {
        [Required]
        public int BuildingId { get; set; }
        
        [Required]
        public string RoomCode { get; set; } = string.Empty;
        
        [Required]
        public string RoomType { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }
        
        [Range(0, 20)]
        public int CurrentOccupancy { get; set; }
        
        public string Status { get; set; } = "Available";
        
        [Required]
        public decimal Price { get; set; }
    }

    public class UpdateRoomDto
    {
        public string? RoomType { get; set; }
        
        [Range(1, 20)]
        public int? Capacity { get; set; }
        
        [Range(0, 20)]
        public int? CurrentOccupancy { get; set; }
        
        public string? Status { get; set; }
        
        public decimal? Price { get; set; }
    }
}
