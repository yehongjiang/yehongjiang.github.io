using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.ViewModels
{
    public class ShowElectrical
    {
        public int id { get; set; }
        public string technology_name { get; set; }
        public string elec_name { get; set; }
        public string remarks { get; set; }
        public string elec_power { get; set; }
        public string pic_url { get; set; }
    }
}