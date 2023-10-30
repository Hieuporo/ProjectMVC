using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectMVC.Data;
using ProjectMVC.DbInitializer;
using ProjectMVC.Models;

namespace ProjectMVC.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DataContext db
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }
        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {

            }

            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole("User")).GetAwaiter().GetResult();
                //if roles are not created, then we will create admin user as well

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Admin",
                    Address = "Utc Ha Noi"
                }, "Admin123*").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");

                _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();

            }
            return;
        }
    }
}
