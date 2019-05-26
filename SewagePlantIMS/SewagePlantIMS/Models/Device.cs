using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class Device
    {
        public int id { get; set; }
        [Display(Name = "工艺段")]
        public int technology_id { get; set; }
        [Display(Name = "设备类别")]
        public int class_id { get; set; }
        [Display(Name = "设备名称")]
        public string title { get; set; }
        [Display(Name = "主要参数")]
        public string summary { get; set; }
        [Display(Name = "设备状态")]
        public int state { get; set; }
        [Display(Name = "添加时间")]
        public DateTime add_date { get; set; }
        [Display(Name = "添加的用户ID")]
        public int add_user_id { get; set; }
        [Display(Name = "添加的用户名")]
        public string add_user_name { get; set; }
        public string pic_src { get; set; }
        [Display(Name = "设备别名")]
        public string alias { get; set; }
        [Display(Name = "安装日期")]
        public DateTime? purchase_date { get; set; }
        [Display(Name = "品牌ID")]
        public int brand_id { get; set; }
        [Display(Name = "型号")]
        public string device_model { get; set; }
        [Display(Name = "功率")]
        public string device_power { get; set; }
        public string qrcode { get; set; }

    }
}