﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.LocalModel
{
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string id { get; set; }
    }
}
