namespace API.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialFieldconstrII : DbMigration
    {
        public override void Up()
        {
            AddColumn("FarmaticCentral.Venta", "EsUltimoRecibido", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("FarmaticCentral.Venta", "EsUltimoRecibido");
        }
    }
}
