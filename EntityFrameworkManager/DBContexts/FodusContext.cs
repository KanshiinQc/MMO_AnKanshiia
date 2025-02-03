using Microsoft.EntityFrameworkCore;


namespace SERVER.Contexts
{
    public class FodusContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=tcp:ttc-dev-sql-server.database.windows.net,1433;Initial Catalog=ttc;Persist Security Info=False;User ID=ttc_admin;Password=IAmTheOwner123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }

    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public float PlayerDisconnectedAtPosX { get; set; }
        public float PlayerDisconnectedAtPosY { get; set; }
    }
}
