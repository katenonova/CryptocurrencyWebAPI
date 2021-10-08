using System.Linq;
using System.Threading.Tasks;
using WebAPI.Context;
using WebAPI.Entities;
using WebAPI.Helpers;

namespace WebAPI.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly APIDbContext _context;
        public UserService(APIDbContext context)
        {
            this._context = context;
        }
        /**
        * This method checks is user exists in the database
        * param: username 
        * param: password
        * return: if user doesn't exists in the database -> return null
        * if user exists in the database -> return user data
        */
        public async Task<User> Authenticate(string username, string password)
        {
            User currentUser = await Task.Run(() => this._context.Users.Where(u => u.Username == username && u.Password == ExtensionMethods.ComputeSha256Hash(password)).FirstOrDefault());
            // return null if user not found
            if (currentUser == null)
                return null;

            // authentication successful so return user details without password
            currentUser.Password = null;
            return currentUser;
        }
    }
}
