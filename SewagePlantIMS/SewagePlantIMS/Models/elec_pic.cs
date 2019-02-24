using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class elec_pic
    {
        public int id { get; set; }
        public string pic_name { get; set; }
        public string pic_url { get; set; }
        public string add_date { get; set; }
        public int elec_id { get; set; }
    }
}