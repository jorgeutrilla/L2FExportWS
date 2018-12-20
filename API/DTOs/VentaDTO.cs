using API.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace API.DTOs
{
    public class VentaDTO
    {
        [Key, Column(Order = 1)] public int IdentificadorVenta { get; set; } // ID
        public DateTime FechaVenta { get; set; }
        [Key, Column(Order = 2)] public int IdLinea { get; set; }
        [MaxLength(6)] public string CodProducto { get; set; }
        [MaxLength(48)] public string DescProducto { get; set; }
        public int CantidadVendida { get; set; }
        [Range(0, 9999)]
        public double PVP { get; set; }
        [Column(TypeName = "char")]
        [StringLength(1)]
        public string TipoVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public int LoteOptimo { get; set; }
        [MaxLength(5)] public string CodLaboratorio { get; set; }
        public bool EsGenerico { get; set; }
        [MaxLength(50)] public string NombreLaboratorio { get; set; }

        public static bool TryFromDBRecord(DbDataReader dbRecord, out VentaDTO ventaDTO)
        {
            bool retVal = true;
            ventaDTO = new VentaDTO();

            if (int.TryParse(dbRecord[nameof(VentaDTO.CantidadVendida)].ToString(), out int cantidad))
                ventaDTO.CantidadVendida = cantidad;
            else
                retVal = false;

            ventaDTO.CodLaboratorio = dbRecord[nameof(VentaDTO.CodLaboratorio)].ToString();
            ventaDTO.CodProducto = dbRecord[nameof(VentaDTO.CodProducto)].ToString();

            if (int.TryParse(dbRecord[nameof(VentaDTO.EsGenerico)].ToString(), out int generico))
                ventaDTO.EsGenerico = generico == 1 ? true : false;
            else
                retVal = false;

            if (DateTime.TryParse(dbRecord[nameof(VentaDTO.FechaVenta)].ToString(), out DateTime fechaVenta))
                ventaDTO.FechaVenta = fechaVenta;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.IdentificadorVenta)].ToString(), out int identificadorVenta))
                ventaDTO.IdentificadorVenta = identificadorVenta;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.IdLinea)].ToString(), out int idLinea))
                ventaDTO.IdLinea = idLinea;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.LoteOptimo)].ToString(), out int lotOpt))
                ventaDTO.LoteOptimo = lotOpt;
            else
                retVal = false;

            ventaDTO.NombreLaboratorio = dbRecord[nameof(VentaDTO.NombreLaboratorio)].ToString();

            if (double.TryParse(dbRecord[nameof(VentaDTO.PVP)].ToString(), out double pvp))
                ventaDTO.PVP = pvp;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.StockActual)].ToString(), out int stAct))
                ventaDTO.StockActual = stAct;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.StockMaximo)].ToString(), out int stMax))
                ventaDTO.StockMaximo = stMax;
            else
                retVal = false;

            if (int.TryParse(dbRecord[nameof(VentaDTO.StockMinimo)].ToString(), out int stMin))
                ventaDTO.StockMinimo = stMin;
            else
                retVal = false;

            ventaDTO.TipoVenta = dbRecord[nameof(VentaDTO.TipoVenta)].ToString();

            return retVal;
        }

        internal static VentaDTO RandomDTO()
        {
            var random = new Random();
            int defaultInt = random.Next(1000);
            double defaultDou = random.NextDouble() * 10;
            return new VentaDTO
            {
                IdentificadorVenta = 0,
                IdLinea = 0,
                CantidadVendida = defaultInt,
                CodLaboratorio = "CodLaboratorio",
                CodProducto = "CodProducto",
                DescProducto = "DescProducto",
                EsGenerico = (random.Next(10) > 5) ? true : false,
                FechaVenta = DateTime.Now.AddDays(random.Next(10) * -1),
                LoteOptimo = random.Next(10),
                NombreLaboratorio = "NombreLaboratorio",
                PVP = defaultDou,
                StockActual = defaultInt,
                StockMaximo = defaultInt,
                StockMinimo = 1,
                TipoVenta = (random.Next(10) > 5) ? "L" : "R"
            };
        }

        internal static Venta ToVenta(VentaDTO ventaDTO)
        {
            return new Venta
            {
                CantidadVendida = ventaDTO.CantidadVendida,
                CodLaboratorio = ventaDTO.CodLaboratorio,
                CodProducto = ventaDTO.CodProducto,
                DescProducto = ventaDTO.DescProducto,
                EsGenerico = ventaDTO.EsGenerico,
                EsUltimoRecibido = true,
                FechaRecibido = DateTime.Now,
                FechaVenta = ventaDTO.FechaVenta,
                IdentificadorVenta = ventaDTO.IdentificadorVenta,
                IdLinea = ventaDTO.IdLinea,
                LoteOptimo = ventaDTO.LoteOptimo,
                NombreLaboratorio = ventaDTO.NombreLaboratorio,
                PVP = ventaDTO.PVP,
                StockActual = ventaDTO.StockActual,
                StockMaximo = ventaDTO.StockMaximo,
                StockMinimo = ventaDTO.StockMinimo,
                TipoVenta = ventaDTO.TipoVenta
            };
        }

        internal static bool TryToDTO(Venta ventaDB, out VentaDTO ventaDTO)
        {
            try
            {
                ventaDTO = new VentaDTO
                {
                    CantidadVendida = ventaDB.CantidadVendida,
                    CodLaboratorio = ventaDB.CodLaboratorio,
                    CodProducto = ventaDB.CodProducto,
                    DescProducto = ventaDB.DescProducto,
                    EsGenerico = ventaDB.EsGenerico,
                    FechaVenta = ventaDB.FechaVenta,
                    IdentificadorVenta = ventaDB.IdentificadorVenta,
                    IdLinea = ventaDB.IdLinea,
                    LoteOptimo = ventaDB.LoteOptimo,
                    NombreLaboratorio = ventaDB.NombreLaboratorio,
                    PVP = ventaDB.PVP,
                    StockActual = ventaDB.StockActual,
                    StockMaximo = ventaDB.StockMaximo,
                    StockMinimo = ventaDB.StockMinimo,
                    TipoVenta = ventaDB.TipoVenta
                };
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}