using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL;
using Domain;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private readonly OnlineStoreDBContext _context;

        public UserService(OnlineStoreDBContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddUser(User user)
        {
            user.UserType = UserType.User;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserById(Guid id, User updatedUser)
        {
            updatedUser.Id = id;
            _context.Users.Update(updatedUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserById(Guid id)
        {
            _context.Users.Remove(new User { Id = id });
            await _context.SaveChangesAsync();
        }
    }
}
