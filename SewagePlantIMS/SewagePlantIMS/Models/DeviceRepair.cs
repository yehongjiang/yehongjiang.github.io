
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
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据不正确！")]
        [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "输入的数据不正确！")]
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
        [Display(Name = "故障/改造时间")]
        public DateTime repair_begin { get; set; }
        [Display(Name = "预计开工时间")]
        public DateTime repair_starts { get; set; }
        [Display(Name = "预计耗时(小时)")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据不正确！")]
        [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "输入的数据不正确！")]
        public double repair_consume { get; set; }
        [Display(Name = "经理意见")]
        public string manager_opinion { get; set; }
        [Display(Name = "是否审批")]
        public int isapproval { get; set; }
        [Display(Name = "是否完工")]
        public int isover { get; set; }
    }
}