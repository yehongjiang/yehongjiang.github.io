using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class Electrical
    {
        public int id { get; set; }
        public int technology_id { get; set; }
        public string technology_name { get; set; }
        public string elec_name { get; set; }
        public string remarks { get; set; }
        public string qrcode { get; set; }
    }
}