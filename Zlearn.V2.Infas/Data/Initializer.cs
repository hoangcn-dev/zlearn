using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Infas.Identity;

namespace Zlearn.V2.Infas.Data
{
    public class Initializer
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly RoleManager<AppIdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<Initializer> _logger;

        public Initializer(
            UserManager<AppIdentityUser> userManager,
            RoleManager<AppIdentityRole> roleManager,
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
                _logger.LogInformation("Start migrating V2...");
                await _context.Database.MigrateAsync();

                _logger.LogInformation("Ensure OutboxEvents table has AggregateId column...");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE \"OutboxEvents\" ADD COLUMN IF NOT EXISTS \"AggregateId\" VARCHAR(150) NOT NULL DEFAULT '';");
                await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS \"IX_OutboxEvents_AggregateId\" ON \"OutboxEvents\" (\"AggregateId\");");
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
                    var createRoleResult = await _roleManager.CreateAsync(new AppIdentityRole
                    {
                        Name = nameof(UserRole.Admin),
                        Id = Zlearn.V2.Domain.Common.IdGenerator.Generate("ROL"),
                    });
                    if (!createRoleResult.Succeeded)
                        throw new Exception("Failed to create default admin role.");
                }

                // Default user role
                if (!await _roleManager.RoleExistsAsync(nameof(UserRole.User)))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new AppIdentityRole
                    {
                        Name = nameof(UserRole.User),
                        Id = Zlearn.V2.Domain.Common.IdGenerator.Generate("ROL"),
                    });
                    if (!createRoleResult.Succeeded)
                        throw new Exception("Failed to create default user role.");
                }

                // Default student role
                if (!await _roleManager.RoleExistsAsync(nameof(UserRole.Student)))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new AppIdentityRole
                    {
                        Name = nameof(UserRole.Student),
                        Id = Zlearn.V2.Domain.Common.IdGenerator.Generate("ROL"),
                    });
                    if (!createRoleResult.Succeeded)
                        throw new Exception("Failed to create default student role.");
                }

                // Default teacher role
                if (!await _roleManager.RoleExistsAsync(nameof(UserRole.Teacher)))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new AppIdentityRole
                    {
                        Name = nameof(UserRole.Teacher),
                        Id = Zlearn.V2.Domain.Common.IdGenerator.Generate("ROL"),
                    });
                    if (!createRoleResult.Succeeded)
                        throw new Exception("Failed to create default teacher role.");
                }

                // Default admin account
                var adminUser = await _userManager.FindByEmailAsync("hoangcn.dev@gmail.com");
                if (adminUser is null)
                {
                    var existingUserByUsername = await _userManager.FindByNameAsync("Admin");
                    if (existingUserByUsername != null)
                    {
                        existingUserByUsername.Email = "hoangcn.dev@gmail.com";
                        var updateResult = await _userManager.UpdateAsync(existingUserByUsername);
                        if (!updateResult.Succeeded)
                            throw new Exception("Failed to update existing admin account email.");
                        adminUser = existingUserByUsername;
                    }
                    else
                    {
                        var adminAccount = new AppIdentityUser
                        {
                            Id = Zlearn.V2.Domain.Common.IdGenerator.Generate("ACC"),
                            UserName = "Admin",
                            FirstName = "Admin",
                            LastName = "System",
                            NickName = "Bình nước màu xanh",
                            IsShowNickName = true,
                            Email = "hoangcn.dev@gmail.com",
                            EmailConfirmed = true,
                            ImageUrl = null,
                            LastLogin = DateTimeOffset.UtcNow
                        };
                        var createAdminResult = await _userManager.CreateAsync(adminAccount, EnvVariableHelper.GetValue(EnvVariableNames.ADMIN_PASSWORD));
                        if (!createAdminResult.Succeeded)
                            throw new Exception("Failed to create default admin account.");
                        adminUser = adminAccount;
                    }
                }

                if (adminUser != null && !await _userManager.IsInRoleAsync(adminUser, nameof(UserRole.Admin)))
                {
                    var assignRoleResult = await _userManager.AddToRoleAsync(adminUser, nameof(UserRole.Admin));
                    if (!assignRoleResult.Succeeded)
                        throw new Exception("Failed to assign admin role to hoangcn.dev@gmail.com.");
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
