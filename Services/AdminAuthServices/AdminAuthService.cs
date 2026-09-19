using FoodMart.Entities;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace FoodMart.Services.AdminAuthServices
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IMongoCollection<AdminUser> _adminCollection;
        private readonly PasswordHasher<AdminUser> _passwordHasher;

        public AdminAuthService(IMongoDatabase database)
        {
            _adminCollection = database.GetCollection<AdminUser>("AdminUsers");

            _passwordHasher = new PasswordHasher<AdminUser>();
        }

        public async Task<AdminUser?> ValidateUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var admin = await _adminCollection
                .Find(x =>
                    x.Email == normalizedEmail &&
                    x.IsActive)
                .FirstOrDefaultAsync();

            if (admin is null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (result ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                admin.PasswordHash = _passwordHasher.HashPassword(admin, password);

                await _adminCollection.ReplaceOneAsync(x => x.Id == admin.Id, admin);
            }

            return admin;
        }

        public async Task<AdminUser> CreateAdminAsync(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Yönetici adı boş olamaz.", nameof(fullName));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("E-posta adresi boş olamaz.", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Şifre boş olamaz.", nameof(password));
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var existingAdmin = await _adminCollection .Find(x => x.Email == normalizedEmail).AnyAsync();

            if (existingAdmin)
            {
                throw new InvalidOperationException("Bu e-posta adresine ait bir yönetici zaten mevcut.");
            }

            var admin = new AdminUser
            {
                FullName = fullName.Trim(),
                Email = normalizedEmail,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            admin.PasswordHash =
                _passwordHasher.HashPassword(
                    admin,
                    password);

            await _adminCollection.InsertOneAsync(admin);

            return admin;
        }

        public async Task<bool> AdminExistsAsync()
        {
            return await _adminCollection.Find(_ => true).AnyAsync();
        }
    }
}