
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class DeviceRepair
    {
        public int id { get; set; }
        [Display(Name = "设备名称")]
        public int device_id { get; set; }
        [Display(Name = "用户")]
        public int user_id { get; set; }
        [Display(Name = "工艺段")]
        public int technology_id { get; set; }
        [Display(Name = "维修日期")]
        public DateTime repair_date { get; set; }
        [Display(Name = "完工日期")]
        public DateTime repair_finsh { get; set; }
        [Display(Name = "维修类别")]
        public string repair_class { get; set; }
        [Display(Name = "维修设备详名")]
        public string repair_title { get; set; }
        [Display(Name = "维修设备数量")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public int repair_nums { get; set; }
        [Display(Name = "维修原因")]
        public string repair_reasons { get; set; }
        [Display(Name = "维修结论")]
        public string repair_conclusion { get; set; }
        [Display(Name = "参与维修的人员")]
        public string repair_join { get; set; }
        [Display(Name = "零部件消耗情况")]
        public string repair_consumption { get; set; }
        [Display(Name = "备注")]
        public string repair_mark { get; set; }
    }
}