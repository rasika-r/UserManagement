using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters.")]
        public string Department { get; set; } = string.Empty;
    }
}
