using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceMaintenancePlan
    {
        public int id { get; set; }
        [Display(Name = "设备名称")]
        public int device_id { get; set; }
        public int user_id { get; set; }
        public int order_id { get; set; }
        [Display(Name = "保养内容")]
        public string dmp_content { get; set; }
        [Display(Name = "物品使用")]
        public string dmp_consumption { get; set; }
        [Display(Name = "保养进度")]
        public int dmp_isfinish { get; set; }
        [Display(Name = "保养月份")]
        public int dmp_month { get; set; }
        [Display(Name = "保养周")]
        public int dmp_weekend { get; set; }
        [Display(Name = "备注")]
        public string remark { get; set; }

    }
}