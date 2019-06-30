using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceRepairPic
    {
        public int id { get; set; }
        [Display(Name = "图片名称")]
        public string title { get; set; }
        [Display(Name = "图片存放地址")]
        public string pic_url { get; set; }
        [Display(Name = "图片描述")]
        public string describe { get; set; }
        [Display(Name = "添加时间")]
        public DateTime add_date { get; set; }
        [Display(Name = "对应的设备维修")]
        public int repair_id { get; set; }

    }
}