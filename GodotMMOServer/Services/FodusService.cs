using SERVER.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SERVER.Services
{
    internal class FodusService
    {
        private FodusContext _dbContext;

        public FodusService()
        {
            // Instantiate FodusContext to connect to the database
            _dbContext = new FodusContext();
        }

        public void CreateUser(string username, string password)
        {
            // Create a new user object
            var newUser = new User
            {
                Username = username,
                Email = Guid.NewGuid().ToString() + "@test.com",
                Password = password
            };

            // Add the new user to the DbContext
            _dbContext.Users.Add(newUser);

            // Save changes to the database
            _dbContext.SaveChanges();
            Console.WriteLine($"User {username} added successfully.");
        }

        public void GetUsers()
        {
            // Query the database for all users
            var users = _dbContext.Users.ToList();

            foreach (var user in users)
            {
                Console.WriteLine($"User ID: {user.ID}, Username: {user.Username}, Email: {user.Email}");
            }
        }

        public void GetUserById(int userId)
        {
            // Query the database for a specific user by ID
            var user = _dbContext.Users.FirstOrDefault(u => u.ID == userId);

            if (user != null)
            {
                Console.WriteLine($"User Found: ID: {user.ID}, Username: {user.Username}, Email: {user.Email}");
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }

        public void UpdateUser(int userId, string newUsername, string newEmail, string newPassword)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.ID == userId);

            if (user != null)
            {
                user.Username = newUsername;
                user.Email = newEmail;
                user.Password = newPassword;

                // Save changes to the database
                _dbContext.SaveChanges();
                Console.WriteLine($"User {userId} updated successfully.");
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }

        public void DeleteUser(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.ID == userId);

            if (user != null)
            {
                // Remove the user from the DbContext
                _dbContext.Users.Remove(user);

                // Save changes to the database
                _dbContext.SaveChanges();
                Console.WriteLine($"User {userId} deleted successfully.");
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }

        public bool UsernameIsAvailable(string username)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Username == username) is not null;
        }

        public bool ValidateCredentials(string username, string password)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Password == password) is not null;
        }

        public User GetUserByUsername(string username)
        {
            return _dbContext.Users.FirstOrDefault(User => User.Username == username);
        }

        internal void SaveUserData(PlayerCharacter player)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.ID == player.ID);
            if (existingUser != null)
            {
                existingUser.PlayerDisconnectedAtPosX = player.Position.X;
                existingUser.PlayerDisconnectedAtPosY = player.Position.Y;
                _dbContext.SaveChanges();
            }
        }

        public User MapPlayerToUser(PlayerCharacter player)
        {
            return _dbContext.Users.FirstOrDefault(u => u.ID == player.ID);
        }

        internal bool IsUsernameAvailable(string username)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Username == username) is null;
        }

        internal void AddItemToPlayer(int playerId, int v, int quantity)
        {
            throw new NotImplementedException();
        }
    }
}
