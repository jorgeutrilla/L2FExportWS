using API.Business;
using API.DTOs;
using API.Model;
using API.Model.Indentity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace API.Controllers
{
    [Authorize]
    [RoutePrefix("api/ventas")]
    public class VentasController : ApiController
    {
        private FarmaContext db = new FarmaContext();
        private readonly FarmaUser user;
        private readonly VentasProcess ventasProcessor;

        public VentasController()
        {
            var identity = HttpContext.Current.User;
            user = db.Users.Single(u => u.UserName.Equals(identity.Identity.Name));
            ventasProcessor = new VentasProcess(db);
        }

        // GET: api/Ventas
        [ResponseType(typeof(VentaDTO))]
        [Route("", Name = "GetLastVenta")]
        public async Task<IHttpActionResult> GetLastVenta()
        {
            Venta lastOne = await ventasProcessor.LastReceived();
            if (null != lastOne)
            {
                VentaDTO.TryToDTO(lastOne, out VentaDTO venta);
                return Ok(venta);
            }

            // simulating, should be comented out on Prod
            //return Ok(VentaDTO.RandomDTO());

            return NotFound(); // empty db
        }

        // POST: api/Ventas
        [ResponseType(typeof(Venta))]
        [Route("simple", Name = "PostVenta")]
        public async Task<IHttpActionResult> PostVenta(VentaDTO ventaDTO)
        {
            bool created = false;
            Venta venta = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                venta = VentaDTO.ToVenta(ventaDTO);
                created = await ventasProcessor.AddVenta(created, venta);
            }
            catch (DbUnexpectedValidationException valEx)
            {
                return BadRequest(valEx.ToString());
            }catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            if (null != venta)
            {
                if (created)
                    return CreatedAtRoute("GetVenta", new { id = venta.IdentificadorVenta }, venta);
                return Ok(venta);
            }
            else
            {
                return InternalServerError(new Exception("DTO to Model failure."));
            }
        }



        // POST: api/Ventas
        [ResponseType(typeof(ListProcessResult))]
        [Route("bloque", Name = "PostVentaList")]
        public async Task<IHttpActionResult> PostVentaList(ListVentaDTO listVentaDTO)
        {
            ListProcessResult count = new ListProcessResult();
            try
            {
                foreach (var ventaDTO in listVentaDTO.Ventas)
                {
                    bool created = false;
                    var venta = VentaDTO.ToVenta(ventaDTO);
                    created = await ventasProcessor.AddVenta(created, venta);
                    if (created) count.Creations++; else count.Updates++;
                }
                return Ok(count);
            }
            catch (DbUnexpectedValidationException valEx)
            {
                return BadRequest(valEx.ToString());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Ventas/5
        [ResponseType(typeof(VentaLineasDTO))]
        [Route("{id}", Name = "GetVenta")]
        public async Task<IHttpActionResult> GetVenta(int id)
        {
            List<Venta> ventas = await db.Ventas.Where(i => i.IdentificadorVenta.Equals(id)).ToListAsync();
            if (ventas == null || ventas.Count() == 0)
            {
                return NotFound();
            }

            VentaLineasDTO ventaLineas = VentaLineasDTO.FromVentas(ventas);

            return Ok(ventaLineas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VentaExists(int id)
        {
            return db.Ventas.Count(e => e.IdentificadorVenta == id) > 0;
        }

 
    }
}