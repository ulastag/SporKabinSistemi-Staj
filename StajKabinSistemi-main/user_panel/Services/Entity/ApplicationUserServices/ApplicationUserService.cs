using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Helpers;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.ApplicationUserServices
{
    public class ApplicationUserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration) : EntityService<ApplicationUser, int>(context), IApplicationUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly Serilog.ILogger _logger = LoggerHelper.GetManualLogger(configuration);

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth=model.DateOfBirth, // bunu DB de nasıl tanımlarım ?
                CreditBalance = 0
                
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                await _userManager.AddToRoleAsync(user, "Customer");
            }
            return result;
        }

        public async Task<LoginServiceResultViewModel> LoginAsync(LoginViewModel model)
        {
            ApplicationUser? user = model.EmailOrPhone.All(char.IsDigit)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone)
                : await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return new LoginServiceResultViewModel { SignInResult = SignInResult.Failed, ErrorMessage = "Invalid login attempt." };
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (signInResult.Succeeded)
            {
                // Check roles and determine redirection path
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "Index",
                        RedirectController = "Admin",
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email
                    };
                }
                else if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "UserPanel",
                        RedirectController = "Account",
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email
                    };
                }
                else
                {
                    // Default redirection if no specific role match
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "UserPanel",
                        RedirectController = "Account"
                    };
                }
            }

            // If sign-in failed, return the failed result and a generic message
            return new LoginServiceResultViewModel { SignInResult = signInResult, ErrorMessage = "Invalid login attempt." };
        }

        public async Task<List<ApplicationUserViewModel>> GetAllUsersWithRolesAsync(string? search = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.UserName.ToLower().Contains(search));
            }

            var users = await query.ToListAsync();

            var result = new List<ApplicationUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Unknown";

                result.Add(new ApplicationUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    CreditBalance = user.CreditBalance,
                    Role = role
                });
            }

            return result;
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            return new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,
                CreditBalance = user.CreditBalance,
                Role = role
            };
        }

        public async Task<bool> UpdateUserAsync(EditUserViewModel model, EditUserViewModel oldUser, string username)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.CreditBalance = model.CreditBalance;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();

            if (currentRole != model.Role)
            {
                if (currentRole != null)
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                await _userManager.AddToRoleAsync(user, model.Role);
            }


            _logger.Information("User with id {userId} is edited by {adminUser}. " +
                "Changes: First Name='{OldName}' -> '{NewName}', " +
                "Last Name='{OldSName}' -> '{NewSName}', " +
                "Email='{OldEmail}' -> '{NewEmail}', " +
                "Username='{OldUsername}' -> '{NewUsername}', " +
                "PhoneNum='{OldPhoneNum}' -> '{NewPhoneNum}', " +
                "Credit='{OldCredit}' -> '{NewCredit}', " +
                "Role='{OldRole}' -> '{NewRole}'",
                model.Id, username,
                oldUser.FirstName, model.FirstName,
                oldUser.LastName, model.LastName,
                oldUser.Email, model.Email,
                oldUser.UserName, model.UserName,
                oldUser.PhoneNumber, model.PhoneNumber,
                oldUser.CreditBalance, model.CreditBalance,
                oldUser.Role, model.Role
                );

            return true;
        }
        public async Task<IdentityResult> AddCreditAsync(ApplicationUser user, decimal amount)
        {
            if (user == null)
            {
                // Or throw an ArgumentNullException
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null." });
            }

            if (amount <= 0)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Credit amount must be positive." });
            }

            user.CreditBalance += amount;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<bool> DeleteUserAsync(string id, string adminUsername)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            _logger.Information("User deleted with username '{Username}' and id '{userId}' by '{User}'",
                user.UserName, id, adminUsername);
            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
        {
            return await _userManager.GetUserAsync(userPrincipal);
        }

        public string? GetCurrentUserId(ClaimsPrincipal userPrincipal)
        {
            return _userManager.GetUserId(userPrincipal);
        }


        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, ChangePasswordViewModel model)
        {
            return await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        }

        public async Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail)
        {
            user.Email = newEmail;
            user.UserName = newEmail;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdatePhoneNumberAsync(ApplicationUser user, string newPhoneNumber)
        {
            return await _userManager.SetPhoneNumberAsync(user, newPhoneNumber);
        }

        public async Task<IdentityResult> AddCreditAsync(int bookingId, string username, string userId, decimal credit)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.CreditBalance = user.CreditBalance + credit;

            _logger.Information("Booking with id '{bookingId}' has been cancelled by '{User}' and user with id '{userId}' is refunded a balance of '{creditBalance}'",
                bookingId, username, userId, credit);

            return await _userManager.UpdateAsync(user);
        }
    }

}
