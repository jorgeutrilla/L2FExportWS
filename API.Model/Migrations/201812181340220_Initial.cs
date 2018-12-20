namespace API.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "FarmaticCentral.FarmaRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "FarmaticCentral.FarmaUserRole",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("FarmaticCentral.FarmaRole", t => t.RoleId)
                .ForeignKey("FarmaticCentral.FarmaUser", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "FarmaticCentral.FarmaUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "FarmaticCentral.FarmaUserClaim",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("FarmaticCentral.FarmaUser", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "FarmaticCentral.FarmaUserLogin",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("FarmaticCentral.FarmaUser", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "FarmaticCentral.Venta",
                c => new
                    {
                        IdentificadorVenta = c.Int(nullable: false),
                        IdLinea = c.Int(nullable: false),
                        FechaVenta = c.DateTime(nullable: false),
                        CodProducto = c.String(),
                        DescProducto = c.String(),
                        CantidadVendida = c.Int(nullable: false),
                        PVP = c.Double(nullable: false),
                        StockActual = c.Int(nullable: false),
                        StockMinimo = c.Int(nullable: false),
                        StockMaximo = c.Int(nullable: false),
                        LoteOptimo = c.String(),
                        CodLaboratorio = c.String(),
                        EsGenerico = c.Boolean(nullable: false),
                        NombreLaboratorio = c.String(),
                    })
                .PrimaryKey(t => new { t.IdentificadorVenta, t.IdLinea });
            
        }
        
        public override void Down()
        {
            DropForeignKey("FarmaticCentral.FarmaUserRole", "UserId", "FarmaticCentral.FarmaUser");
            DropForeignKey("FarmaticCentral.FarmaUserLogin", "UserId", "FarmaticCentral.FarmaUser");
            DropForeignKey("FarmaticCentral.FarmaUserClaim", "UserId", "FarmaticCentral.FarmaUser");
            DropForeignKey("FarmaticCentral.FarmaUserRole", "RoleId", "FarmaticCentral.FarmaRole");
            DropIndex("FarmaticCentral.FarmaUserLogin", new[] { "UserId" });
            DropIndex("FarmaticCentral.FarmaUserClaim", new[] { "UserId" });
            DropIndex("FarmaticCentral.FarmaUser", "UserNameIndex");
            DropIndex("FarmaticCentral.FarmaUserRole", new[] { "RoleId" });
            DropIndex("FarmaticCentral.FarmaUserRole", new[] { "UserId" });
            DropIndex("FarmaticCentral.FarmaRole", "RoleNameIndex");
            DropTable("FarmaticCentral.Venta");
            DropTable("FarmaticCentral.FarmaUserLogin");
            DropTable("FarmaticCentral.FarmaUserClaim");
            DropTable("FarmaticCentral.FarmaUser");
            DropTable("FarmaticCentral.FarmaUserRole");
            DropTable("FarmaticCentral.FarmaRole");
        }
    }
}
