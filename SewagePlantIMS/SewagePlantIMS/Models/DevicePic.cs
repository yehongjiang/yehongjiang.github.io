using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DevicePic
    {
        public int id {get;set;}
        public string title { get; set; }
        public string pic_url { get; set; }
        public int state { get; set; }
        public DateTime add_date { get; set; }
        public int device_id { get; set; }

    }
}