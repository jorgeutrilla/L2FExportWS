using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using API.Model;

namespace API.DTOs
{
    public class VentaLineasDTO
    {
        public int IdVenta { get; set; }
        public DateTime FechaVenta { get; set; }

        public List<VentaDTO> Lineas { get; set; }
        public string CodLaboratorio { get; set; }

        internal static VentaLineasDTO FromVentas(List<Venta> ventas)
        {
            var _1st = ventas.First();
            return new VentaLineasDTO
            {
                IdVenta = _1st.IdentificadorVenta,
                FechaVenta = _1st.FechaVenta,
                CodLaboratorio = _1st.CodLaboratorio,
                Lineas = ventas.Select(v => new VentaDTO
                {
                    IdLinea = v.IdLinea,
                    CantidadVendida = v.CantidadVendida,
                    CodProducto = v.CodProducto,
                    DescProducto = v.DescProducto,
                    EsGenerico = v.EsGenerico,
                    LoteOptimo = v.LoteOptimo,
                    NombreLaboratorio = v.NombreLaboratorio,
                    PVP = v.PVP,
                    TipoVenta = v.TipoVenta
                }).ToList()
            };
        }
    }
}