using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class chemical_device_pic
    {
        public int id { get; set; }
        public int cd_id { get; set; }
        public string cd_picname { get; set; }
        public string cd_picurl { get; set; }
        public DateTime cd_addtime { get; set; }

    }
}