namespace API.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialFieldconstr : DbMigration
    {
        public override void Up()
        {
            AddColumn("FarmaticCentral.Venta", "FechaRecibido", c => c.DateTime(nullable: false));
            AlterColumn("FarmaticCentral.Venta", "LoteOptimo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("FarmaticCentral.Venta", "LoteOptimo", c => c.String());
            DropColumn("FarmaticCentral.Venta", "FechaRecibido");
        }
    }
}
