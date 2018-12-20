using API.Model.Indentity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace API.Model
{
    public class FarmaContext : IdentityDbContext<FarmaUser, Role, int, UserLogin, UserRole, UserClaim>
    {

        public DbSet<Venta> Ventas { get; set; }


        public FarmaContext() : base("CentralDB")
        {

        }

        public static FarmaContext Create()
        {
            Database.SetInitializer<FarmaContext>(new DBInitializer());

            return new FarmaContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modify conventions
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();//Workaround
            modelBuilder.HasDefaultSchema("FarmaticCentral");
            base.OnModelCreating(modelBuilder); //this must be called before other rules

            //  Config tablas IF2
            modelBuilder.Entity<FarmaUser>().ToTable(nameof(FarmaUser));
            modelBuilder.Entity<Role>().ToTable("FarmaRole");
            modelBuilder.Entity<UserRole>().ToTable("FarmaUserRole");
            modelBuilder.Entity<UserLogin>().ToTable("FarmaUserLogin");
            modelBuilder.Entity<UserClaim>().ToTable("FarmaUserClaim");
        }

        //This function will ensure the database is created and seeded with any default data.
        //public class DBInitializer : CreateDatabaseIfNotExists<FarmaContext>
        //public class DBInitializer : DropCreateDatabaseAlways<FarmaContext>
        public class DBInitializer : DropCreateDatabaseIfModelChanges<FarmaContext>
        //public class DBInitializer: CreateDatabaseIfNotExists<FarmaContext>
        {
            /// <summary>
            /// Esto no se debería usar en prod
            /// </summary>
            /// <param name="context"></param>
            protected override void Seed(FarmaContext context)
            {
                var roleManager = new RoleManager<Role, int>(new RoleStore<Role, int, UserRole>(context));
                var userManager = new UserManager<FarmaUser, int>(new UserStore<FarmaUser, Role, int, UserLogin, UserRole, UserClaim>(context));

                // role admin
                string adminRoleName = "Administrator";
                roleManager.Create(new Role() { Name = adminRoleName });

                // create admin User
                var admin = new FarmaUser
                {
                    Email = "soporte@makesoft.es",
                    UserName = "soporte@makesoft.es",
                };
                var success = userManager.Create(admin);
                if (success.Succeeded)
                {
                    userManager.AddToRole(admin.Id, adminRoleName);
                }
                userManager.AddPassword(admin.Id, "Qwerty.123$");
                context.SaveChanges();
            }
        }

    }
}
