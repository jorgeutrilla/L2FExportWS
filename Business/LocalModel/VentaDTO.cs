using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Business.LocalModel
{
    public class VentaDTO
    {
        public int IdentificadorVenta { get; set; } // ID
        public DateTime FechaVenta { get; set; }
        public int IdLinea { get; set; }
        public string CodProducto { get; set; }
        public string DescProducto { get; set; }
        public int CantidadVendida { get; set; }
        public double PVP { get; set; }
        public string TipoVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public int LoteOptimo { get; set; }
        public string CodLaboratorio { get; set; }
        public bool EsGenerico { get; set; }
        public string NombreLaboratorio { get; set; }

        public override string ToString()
        {
            return $"{FechaVenta}|{IdentificadorVenta}|{IdLinea}|{CodProducto}|{DescProducto}|{CantidadVendida}|{PVP}|{TipoVenta}|{StockActual}|{StockMinimo}|{StockMaximo}|{LoteOptimo}|{CodLaboratorio}|{EsGenerico}|{NombreLaboratorio}";
        }

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
            ventaDTO.DescProducto = dbRecord[nameof(VentaDTO.DescProducto)].ToString();

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
    }
}
