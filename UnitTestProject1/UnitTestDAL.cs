using Business;
using Business.LocalModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace UnitTestProject1
{
	[TestClass]
	public class UnitTestDAL
	{
		//private readonly JobConfig config = null;
		private DbProviderFactory _factory;
		private DbConnection _connection;
		//private readonly int recordsExported = 0;


		private readonly string _countCommand = @"SELECT COUNT(*) FROM Venta WHERE FechaHora <= @fromDate";
		private readonly string _commandSelect = @"Select Venta.FechaHora     as FechaVenta
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

		[TestMethod]
		public void TestInit()
		{
			Assert.IsTrue(this.ConnectionOpen(true));
		}

		[TestMethod]
		public void TestSelectData()
		{
			int total = 0;
			try
			{
				this.ConnectionOpen(false);

				var command = _connection.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = _countCommand;
				var twoDaysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(2));
				SqlParameter param = new SqlParameter
				{
					ParameterName = "@fromDate",
					Value = twoDaysAgo
				};
				command.Parameters.Add(param);

				total = (int)command.ExecuteScalar(); // total number of recorsd for pagin

				command = _connection.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = _commandSelect;
				int daysToRepeat = 150;
				SqlParameter weeks = new SqlParameter
				{
					ParameterName = "@daysToRepeat",
					Value = daysToRepeat * -1
				};
				command.Parameters.Add(weeks);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var value = reader["IdentificadorVenta"];
						Debug.WriteLine($"{reader["IdentificadorVenta"]}: CodProducto:{reader["CodProducto"]}; IdLinea:{reader["IdLinea"]}");

						// conversion to DTO
						if (VentaDTO.TryFromDBRecord(reader, out VentaDTO venta))
						{
							// convertir a JSON (WebAPI will receive this)
							var ventaJson = JsonConvert.SerializeObject(venta);
							Debug.WriteLine($"DTO en json: {ventaJson}");

						}
						else
							Debug.WriteLine($"ERROR de conversion a DTO en IdVenta {reader["IdentificadorVenta"]}");


					}
				}
			}
			finally
			{
				if (_connection.State == System.Data.ConnectionState.Open)
				{
					_connection.Close();
				}
				_connection.Dispose();
			}
		}



		private DbConnection BuildConnection(string farmaticConnectionString)
		{
			var _connection = _factory.CreateConnection();
			_connection.ConnectionString = farmaticConnectionString;
			return _connection;
		}


		public bool ConnectionOpen(bool close = false)
		{
			bool success = false;
			var configCS = ConfigurationManager.ConnectionStrings["CS_FarmaticDB"];
			JobConfig jc = new JobConfig
			{
				FarmaticConnectionString = configCS.ConnectionString,
				ProviderConnectionString = configCS.ProviderName
			};
			_factory = DbProviderFactories.GetFactory(jc.ProviderConnectionString);
			//using (_connection = BuildConnection(jc.FarmaticConnectionString))
			//{
			_connection = BuildConnection(jc.FarmaticConnectionString);
			try
			{
				_connection.Open();
				Debug.WriteLine($"Connected");
				success = true;
			}
			catch (Exception)
			{
				success = false;
			}
			finally
			{
				if (close)
				{
					_connection.Close();
					Debug.WriteLine($"Disconnected");
				}
			}
			//}

			return success;
		}
	}
}
