using System.ComponentModel.DataAnnotations;

namespace Task_Management_API.DTOs
{
    public class UpdateTaskDto
    {
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Status { get; set; }
    }
}
