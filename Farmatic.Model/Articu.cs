using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmatic.Model
{
    public class Articu
    {
        public int Id { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public string LoteOptimo { get; set; }
        public string Laboratorio { get; set; }
        public string IdArticu { get; set; }
    }
}
