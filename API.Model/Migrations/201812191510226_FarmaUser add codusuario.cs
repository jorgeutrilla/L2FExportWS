namespace API.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FarmaUseraddcodusuario : DbMigration
    {
        public override void Up()
        {
            AddColumn("FarmaticCentral.FarmaUser", "CodUsuario", c => c.String(nullable: false, maxLength: 5));
        }
        
        public override void Down()
        {
            DropColumn("FarmaticCentral.FarmaUser", "CodUsuario");
        }
    }
}
