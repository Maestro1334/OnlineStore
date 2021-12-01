using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IUserService
    {
        public Task<List<User>> GetUsers();
        public Task<User> GetUserById(Guid id);
        public Task AddUser(User user);
        public Task UpdateUserById(Guid id, User updatedUser);
        public Task DeleteUserById(Guid id);
    }

}
