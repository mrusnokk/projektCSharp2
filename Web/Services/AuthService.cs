using BCrypt.Net;
using Shared.Models;
using Web.Repositories;

namespace Web.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Error)> RegisterAsync(
            string email, string password, string firstName, string lastName, string phone)
        {
            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
                return (false, "Uživatel s tímto emailem již existuje.");

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);
            return (true, string.Empty);
        }

        public async Task<(bool Success, User? User, string Error)> LoginAsync(
            string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                return (false, null, "Nesprávný email nebo heslo.");

            if (!user.IsActive)
                return (false, null, "Účet byl deaktivován.");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, null, "Nesprávný email nebo heslo.");

            return (true, user, string.Empty);
        }
    }
}
