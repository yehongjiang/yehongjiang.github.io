using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.ViewModels
{
    public class ShowChemicalDevice
    {
        public int id { get; set; }
        [Display(Name = "仪器名称")]
        public string cd_name { get; set; }
        [Display(Name = "型号")]
        public string cd_model { get; set; }
        [Display(Name = "数量")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public int cd_quantity { get; set; }
        [Display(Name = "编号")]
        public string cd_num { get; set; }
        [Display(Name = "厂家")]
        public string cd_manufacturer { get; set; }
        [Display(Name = "保管人")]
        public string cd_preserver { get; set; }
        [Display(Name = "启用时间")]
        [DataType(DataType.DateTime)]
        public DateTime cd_begin_time { get; set; }
        [Display(Name = "备注")]
        [DataType(DataType.MultilineText)]
        public string cd_remark { get; set; }
        public string cd_picurl { get; set; }

    }
}