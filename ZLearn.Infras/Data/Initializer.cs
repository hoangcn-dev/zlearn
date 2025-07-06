using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Utils;
using ZLearn.Infras.Identity;

namespace ZLearn.Infras.Data
{
    public class Initializer
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<Initializer> _logger;

        public Initializer(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            AppDbContext context,
            ILogger<Initializer> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                Console.WriteLine();
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        public async Task InitializeDataAsync()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Default admin role
                if (!await _roleManager.RoleExistsAsync(nameof(UserRole.Admin)))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new AppRole
                    {
                        Name = nameof(UserRole.Admin),
                        Id = IdGenerator.Generate("ROL"),
                    });
                    if (!createRoleResult.Succeeded)
                        throw new DatabaseErrorException("Failed to create default admin role.");
                }

                // Default admin account
                if (!await _userManager.Users.AnyAsync(u => u.UserName == nameof(UserRole.Admin)))
                {
                    var adminAccount = new AppUser
                    {
                        Id = IdGenerator.Generate("ACC"),
                        UserName = nameof(UserRole.Admin),
                        Email = "dever.z.ckpt.526@gmail.com",
                        EmailConfirmed = true,
                        ImagePath = "https://res.cloudinary.com/dvk5yt0oi/image/upload/v1751492653/b2e0meuozqt0ti4r7her_qhbicx.jpg"
                    };
                    var createAdminResult = await _userManager.CreateAsync(adminAccount, "Admin@123");
                    if (!createAdminResult.Succeeded)
                        throw new DatabaseErrorException("Failed to create default admin account.");
                    var assignRoleResult = await _userManager.AddToRoleAsync(adminAccount, nameof(UserRole.Admin));
                    if (!assignRoleResult.Succeeded)
                        throw new DatabaseErrorException("Failed to assign admin role.");
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
