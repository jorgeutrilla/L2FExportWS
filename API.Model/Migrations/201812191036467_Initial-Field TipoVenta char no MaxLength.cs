namespace API.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialFieldTipoVentacharnoMaxLength : DbMigration
    {
        public override void Up()
        {
            AddColumn("FarmaticCentral.Venta", "TipoVenta", c => c.String(maxLength: 1, fixedLength: true, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("FarmaticCentral.Venta", "TipoVenta");
        }
    }
}
