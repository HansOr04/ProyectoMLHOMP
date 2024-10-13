using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserViewModel> GetUserByIdAsync(string userId);
        Task<UserViewModel> GetUserByEmailAsync(string email);
        Task<UserViewModel> CreateUserAsync(UserViewModel userViewModel);
        Task<UserViewModel> UpdateUserAsync(string userId, UserViewModel userViewModel);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();
        Task<IEnumerable<UserViewModel>> GetUsersByRoleAsync(string role);
        Task<bool> AddUserToRoleAsync(string userId, string role);
        Task<bool> RemoveUserFromRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> LockUserAsync(string userId);
        Task<bool> UnlockUserAsync(string userId);
        Task<DateTime?> GetLastLoginDateAsync(string userId);
        Task<bool> UpdateLastLoginDateAsync(string userId);
    }
}