using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Business.LocalModel
{
    public class GetLastResult
    {
        public HttpStatusCode Status { get; set; }
        public DateTime DateLastVenta { get; set; }
    }
}
