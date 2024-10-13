using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Data;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.Services.Interfaces;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly DatabaseProyecto _context;

        public UserService(UserManager<User> userManager, DatabaseProyecto context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<UserViewModel> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? MapUserToViewModel(user) : null;
        }

        public async Task<UserViewModel> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null ? MapUserToViewModel(user) : null;
        }

        public async Task<UserViewModel> CreateUserAsync(UserViewModel userViewModel)
        {
            var user = new User
            {
                UserName = userViewModel.Email,
                Email = userViewModel.Email,
                FirstName = userViewModel.FirstName,
                LastName = userViewModel.LastName,
                DateOfBirth = userViewModel.DateOfBirth,
                Address = userViewModel.Address,
                City = userViewModel.City,
                Country = userViewModel.Country,
                IsHost = userViewModel.IsHost,
                ProfilePictureUrl = userViewModel.ProfilePictureUrl,
                IsVerified = false,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, userViewModel.Password);

            if (result.Succeeded)
            {
                return MapUserToViewModel(user);
            }

            throw new Exception($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task<UserViewModel> UpdateUserAsync(string userId, UserViewModel userViewModel)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            user.FirstName = userViewModel.FirstName;
            user.LastName = userViewModel.LastName;
            user.DateOfBirth = userViewModel.DateOfBirth;
            user.Address = userViewModel.Address;
            user.City = userViewModel.City;
            user.Country = userViewModel.Country;
            user.IsHost = userViewModel.IsHost;
            user.ProfilePictureUrl = userViewModel.ProfilePictureUrl;
            user.HostBio = userViewModel.HostBio;
            user.HostLanguages = userViewModel.HostLanguages;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return MapUserToViewModel(user);
            }

            throw new Exception($"User update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) == null;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users.Select(MapUserToViewModel);
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersByRoleAsync(string role)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            return usersInRole.Select(MapUserToViewModel);
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }

        public async Task<DateTime?> GetLastLoginDateAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.LockoutEnd?.UtcDateTime;
        }

        public async Task<bool> UpdateLastLoginDateAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.LockoutEnd = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        private UserViewModel MapUserToViewModel(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                IsHost = user.IsHost,
                RegistrationDate = user.RegistrationDate,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsVerified = user.IsVerified,
                HostBio = user.HostBio,
                HostLanguages = user.HostLanguages,
                IdVerified = user.IdVerified,
                EmailVerified = user.EmailVerified,
                PhoneVerified = user.PhoneVerified
            };
        }
    }
}