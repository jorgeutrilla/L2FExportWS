using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmatic.Model
{
    public class LineaVenta
    {
        [Key]
        public int IdNLinea { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string TipoAportacion { get; set; }
        public int Cantidad { get; set; }
        public double Pvp { get; set; }
    }
}
