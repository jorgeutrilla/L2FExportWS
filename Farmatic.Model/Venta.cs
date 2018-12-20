using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmatic.Model
{
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
