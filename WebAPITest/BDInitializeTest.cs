using System;
using API.Model;
using API.Model.Indentity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAPITest
{
    [TestClass]
    public class BDInitializeTest
    {
        private FarmaContext _context;
        private RoleManager<Role, int> _roleManager;
        private UserManager<FarmaUser, int> _userManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new FarmaContext();
            _roleManager = new RoleManager<Role, int>(new RoleStore<Role, int, UserRole>(_context));
            _userManager = new UserManager<FarmaUser, int>(new UserStore<FarmaUser, Role, int, UserLogin, UserRole, UserClaim>(_context));
        }

        [TestMethod]
        public void BDInit()
        {
            if (_roleManager.FindByName("Administrator") != null)
            {
                Assert.Fail("DB is already initialized.");
                return;
            }

            // role admin
            string adminRoleName = "Administrator";
            _roleManager.Create(new Role() { Name = adminRoleName });

            // create admin User
            var admin = new FarmaUser
            {
                Email = "soporte@makesoft.es",
                UserName = "soporte@makesoft.es",
            };
            var success = _userManager.Create(admin);
            if (success.Succeeded)
            {
                _userManager.AddToRole(admin.Id, adminRoleName);
            }
            _userManager.AddPassword(admin.Id, "Qwerty.123$");

            _context.SaveChanges();
        }
    }
}
