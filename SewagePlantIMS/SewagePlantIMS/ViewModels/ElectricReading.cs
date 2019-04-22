using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.ViewModels
{
    public class ElectricReading
    {
        //数据库主表内容
        public int total_id { get; set; }
        public DateTime add_time { get; set; }
        public string  title { get; set; } //一直未有数据
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string remark { get; set; }
        //其他检查里的内容
        [Display(Name = "直流屏运行情况")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int direct_current_state { get; set; }
        [Display(Name = "监控室运行情况")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int control_room_state { get; set; }
        [Display(Name = "配电柜门")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int distributor_door_state { get; set; }
        [Display(Name = "配电房窗")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int electric_room_state { get; set; }
        [Display(Name = "照明")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int light_state { get; set; }
        [Display(Name = "卫生")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int wash_state { get; set; }
        [Display(Name = "消防器械")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int fire_epuip_state { get; set; }
        [Display(Name = "安全器具")]
        [Required(ErrorMessage = "该字段不能为空！")]
        public int safe_appliance_state { get; set; }
        [Display(Name = "其他")]
        public string mark { get; set; }
        //低压1#线
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_c { get; set; }
        [Display(Name = "功率因素(%)")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_pf { get; set; }
        //低压2#线
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_c { get; set; }
        [Display(Name = "功率因素(%)")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_pf { get; set; }
        //低压母联
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_c { get; set; }
        //高压1线
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_c { get; set; }
        //高压2#线
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_c { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_c { get; set; }
        //电表1#线
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_all { get; set; }
        [Display(Name = "尖")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_sharp { get; set; }
        [Display(Name = "峰")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_peak { get; set; }
        [Display(Name = "平")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_shoulder { get; set; }
        [Display(Name = "谷")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_offpeak { get; set; }
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_all { get; set; }
        [Display(Name = "尖")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_sharp { get; set; }
        [Display(Name = "峰")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_peak { get; set; }
        [Display(Name = "平")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_shoulder { get; set; }
        [Display(Name = "谷")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_offpeak { get; set; }
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_all { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_c { get; set; }
        //电表2#线
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_all { get; set; }
        [Display(Name = "尖")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_sharp { get; set; }
        [Display(Name = "峰")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_peak { get; set; }
        [Display(Name = "平")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_shoulder { get; set; }
        [Display(Name = "谷")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_offpeak { get; set; }
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_all { get; set; }
        [Display(Name = "尖")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_sharp { get; set; }
        [Display(Name = "峰")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_peak { get; set; }
        [Display(Name = "平")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_shoulder { get; set; }
        [Display(Name = "谷")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_offpeak { get; set; }
        [Display(Name = "总")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_all { get; set; }
        [Display(Name = "A")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_a { get; set; }
        [Display(Name = "B")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_b { get; set; }
        [Display(Name = "C")]
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_c { get; set; }
    }
}