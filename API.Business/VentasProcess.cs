using API.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Business
{
    public class VentasProcess
    {
        private readonly FarmaContext _context;
        public VentasProcess(FarmaContext context)
        {
            _context = context;
        }

        public async Task<Venta> LastReceived()
        {
            var lastOne = await _context.Ventas.Where(
                    v => v.EsUltimoRecibido == true
                ).SingleOrDefaultAsync();
            if (null == lastOne)
            {
                lastOne = await _context.Ventas.Take(1)
                    .OrderByDescending(e => e.FechaVenta)
                    .FirstOrDefaultAsync();
            }

            return lastOne;
        }

        public async Task<bool> AddVenta(bool created, Venta venta)
        {
            Venta lastOne = await LastReceived();

            // find venta
            var existingVenta = _context.Ventas.Find(venta.IdentificadorVenta, venta.IdLinea);
            if (null == existingVenta)
            {
                if (null != lastOne && lastOne.EsUltimoRecibido)
                {
                    lastOne.EsUltimoRecibido = false;
                }

                _context.Ventas.Add(venta);
                created = true;
            }
            else
            {
                // update existing with incoming
                existingVenta.CantidadVendida = venta.CantidadVendida;
                existingVenta.CodLaboratorio = venta.CodLaboratorio;
                existingVenta.CodProducto = venta.CodProducto;
                existingVenta.DescProducto = venta.DescProducto;
                existingVenta.EsGenerico = venta.EsGenerico;
                existingVenta.FechaVenta = venta.FechaVenta;
                existingVenta.LoteOptimo = venta.LoteOptimo;
                existingVenta.NombreLaboratorio = venta.NombreLaboratorio;
                existingVenta.PVP = venta.PVP;
                existingVenta.StockActual = venta.StockActual;
                existingVenta.StockMaximo = venta.StockMaximo;
                existingVenta.StockMinimo = venta.StockMinimo;
                existingVenta.TipoVenta = venta.TipoVenta;
                existingVenta.FechaRecibido = DateTime.Now;
            }


            await _context.SaveChangesAsync();
            return created;
        }
    }
}
