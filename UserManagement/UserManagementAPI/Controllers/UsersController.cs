using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using UserManagementAPI.Repository;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Retrieving all users");
                var users = await _userRepository.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving users." });
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "User ID must be greater than 0." });

                _logger.LogInformation("Retrieving user with ID: {UserId}", id);
                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the user." });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Creating new user with email: {Email}", createUserDto.Email);

                // Check if email already exists
                var emailExists = await _userRepository.EmailExistsAsync(createUserDto.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Attempt to create user with existing email: {Email}", createUserDto.Email);
                    return BadRequest(new { message = "A user with this email already exists." });
                }

                var user = new User
                {
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    Department = createUserDto.Department
                };

                var createdUser = await _userRepository.CreateUserAsync(user);
                _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);

                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the user." });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "User ID must be greater than 0." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Updating user with ID: {UserId}", id);

                var userExists = await _userRepository.UserExistsAsync(id);
                if (!userExists)
                {
                    _logger.LogWarning("User with ID {UserId} not found for update", id);
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                // Check if email is being changed and if new email already exists
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser != null && !existingUser.Email.Equals(updateUserDto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userRepository.EmailExistsAsync(updateUserDto.Email);
                    if (emailExists)
                    {
                        _logger.LogWarning("Attempt to update user with existing email: {Email}", updateUserDto.Email);
                        return BadRequest(new { message = "A user with this email already exists." });
                    }
                }

                var user = new User
                {
                    FirstName = updateUserDto.FirstName,
                    LastName = updateUserDto.LastName,
                    Email = updateUserDto.Email,
                    Department = updateUserDto.Department
                };

                var updatedUser = await _userRepository.UpdateUserAsync(id, user);
                _logger.LogInformation("User with ID {UserId} updated successfully", id);

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the user." });
            }
        }

        /// <summary>
        /// Delete a user by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "User ID must be greater than 0." });

                _logger.LogInformation("Deleting user with ID: {UserId}", id);

                var deleted = await _userRepository.DeleteUserAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                _logger.LogInformation("User with ID {UserId} deleted successfully", id);
                return Ok(new { message = $"User with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the user." });
            }
        }
    }
}
