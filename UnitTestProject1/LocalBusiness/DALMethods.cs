using Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1.LocalBusiness
{
    public class DALMethods
    {

        public readonly string _parametrizedSelectVentas = @"Select Venta.FechaHora     as FechaVenta
        , Venta.IdVenta          as IdentificadorVenta       
        , Linea.IdNLinea     as IdLinea
        , Linea.Codigo       as CodProducto
        , Linea.Descripcion  as DescProducto
        , Linea.Cantidad     as CantidadVendida
        , Linea.Pvp          as PVP
        , case when (Linea.TipoAportacion <> '') then 'R' else 'L' end as TipoVenta
        , Producto.StockActual
        , Producto.StockMinimo
        , Producto.StockMaximo
        , Producto.LoteOptimo as LoteOptimo
        , Producto.Laboratorio as CodLaboratorio
        , Cast(IsNull(Prod2.efg, 0) as int) as EsGenerico
        , case when ( IsNull(Lab.Nombre, '') = '' ) then Labcon.Nombre else Lab.Nombre end as NombreLaboratorio
from Venta inner join LineaVenta           as Linea on Linea.IdVenta = Venta.IdVenta
    inner join Articu               as Producto on Producto.IdArticu = Linea.Codigo
    left join Laboratorio           as Lab on Lab.Codigo = Producto.Laboratorio
    left join Consejo.dbo.Labor   as Labcon on LabCon.Codigo = Producto.Laboratorio
    left join ArticuAux           as Prod2 on Prod2.CodigoArt = Producto.IdArticu
	where FechaHora > (SELECT DATEADD(day, @daysToRepeat, CURRENT_TIMESTAMP))
	order by FechaHora";


        private JobConfig _config;
        public DALMethods(JobConfig config)
        {
            _config = config;
        }

        public bool ConnectionOpen(out DbConnection connection)
        {
            bool success = false;            
            var factory = DbProviderFactories.GetFactory(_config.ProviderConnectionString);
            connection = BuildConnection(factory, _config.FarmaticConnectionString);
            try
            {
                connection.Open();
                Debug.WriteLine($"Connected");
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        private DbConnection BuildConnection(DbProviderFactory factory, string farmaticConnectionString)
        {
            var _connection = factory.CreateConnection();
            _connection.ConnectionString = farmaticConnectionString;
            return _connection;
        }
    }
}
