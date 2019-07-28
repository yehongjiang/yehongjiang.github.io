using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceMaintenance
    {
        public int id { get; set; }
        [Display(Name = "设备名称")]
        public int device_id { get; set; }
        public int user_id { get; set; }
        [Display(Name = "保养内容")]
        public string dm_content { get; set; }
        [Display(Name = "物品使用")]
        public string dm_consumption { get; set; }
        [Display(Name = "保养进度")]
        public int dm_isfinish { get; set; }
        [Display(Name = "保养时间")]
        public DateTime dm_date { get; set; }
        [Display(Name = "保养周")]
        public int dm_weekend { get; set; }
        [Display(Name = "是否额外")]
        public int dm_isextra { get; set; }
        [Display(Name = "备注")]
        public string remark { get; set; }

    }
}