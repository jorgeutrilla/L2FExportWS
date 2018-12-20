using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Model
{
    public class Venta
    {
        [Key, Column(Order = 1)]
        public int IdentificadorVenta { get; set; } // ID
        public DateTime FechaVenta { get; set; }
        [Key, Column(Order = 2)]
        public int IdLinea { get; set; }
        [MaxLength(6)]
        public string CodProducto { get; set; }
        [MaxLength(48)]
        public string DescProducto { get; set; }
        [Range(1,9999)]
        public int CantidadVendida { get; set; }
        public double PVP { get; set; }
        [Column(TypeName = "char")]
        [StringLength(1)]
        public string TipoVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public int LoteOptimo { get; set; }
        [MaxLength(5)]
        public string CodLaboratorio { get; set; }
        public bool EsGenerico { get; set; }
        [MaxLength(50)]
        public string NombreLaboratorio { get; set; }
        public DateTime FechaRecibido { get; set; }
        public bool EsUltimoRecibido { get; set; }
    }
}
