using System;
using DAL;
using Domain;

namespace Service
{
    public interface IUserService
    {
    }

    public class UserService : IUserService
    {
        private readonly OnlineStoreDBContext _context;

        public UserService(OnlineStoreDBContext context)
        {
            _context = context;
        }
    }
}
