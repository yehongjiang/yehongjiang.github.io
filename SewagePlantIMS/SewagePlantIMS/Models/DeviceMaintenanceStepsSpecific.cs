using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceMaintenanceStepsSpecific
    {
        public int id { get; set; }
        public int dms_id { get; set; }
        public int order_id { get; set; }
        public string pic_url { get; set; }
        [Display(Name="描述")]
        public string dmss_describe { get; set; }

    }
}