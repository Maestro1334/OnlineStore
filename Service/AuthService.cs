using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public interface IAuthService
    {
    }

    public class AuthService : IAuthService
    {
        private readonly OnlineStoreDBContext _context;

        public AuthService(OnlineStoreDBContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }
    }
}
