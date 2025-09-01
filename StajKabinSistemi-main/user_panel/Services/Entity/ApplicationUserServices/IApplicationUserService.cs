using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.ApplicationUserServices
{
    public interface IApplicationUserService : IEntityService<ApplicationUser, int>
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<LoginServiceResultViewModel> LoginAsync(LoginViewModel model);
        Task<List<ApplicationUserViewModel>> GetAllUsersWithRolesAsync(string? search);
        Task<EditUserViewModel?> GetUserForEditAsync(string userId);
        Task<bool> UpdateUserAsync(EditUserViewModel model, EditUserViewModel oldUser, string username);
        Task<bool> DeleteUserAsync(string id, string adminUsername);
        Task LogoutAsync();
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, ChangePasswordViewModel model);
        Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail);
        Task<IdentityResult> UpdatePhoneNumberAsync(ApplicationUser user, string newPhoneNumber);
        Task<IdentityResult> AddCreditAsync(ApplicationUser user, decimal amount);
        string? GetCurrentUserId(ClaimsPrincipal userPrincipal);
        Task<IdentityResult> AddCreditAsync(int bookingId, string username, string userId, decimal credit);
    }
}
