using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceMaintenanceSteps
    {
        public int id { get; set; }
        public int device_id { get; set; }
        [Display(Name = "保养名称")]
        public string dms_title { get; set; }
    }
}