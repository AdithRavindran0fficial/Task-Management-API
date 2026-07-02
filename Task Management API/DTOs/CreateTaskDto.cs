using System.ComponentModel.DataAnnotations;

namespace Task_Management_API.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title is mandatory.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "DueDate is required.")]
        public DateTime DueDate { get; set; }
    }
}
