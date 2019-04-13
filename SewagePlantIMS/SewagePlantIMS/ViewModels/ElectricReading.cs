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
        [Required(ErrorMessage = "该字段不能为空！")]
        public int direct_current_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int control_room_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int distributor_door_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int electric_room_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int light_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int wash_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int fire_epuip_state { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        public int safe_appliance_state { get; set; }
        public string mark { get; set; }
        //低压1#线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_v_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_e_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd1_pf { get; set; }
        //低压2#线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_v_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_e_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvd2_pf { get; set; }
        //低压母联
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_v_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double elvs_e_c { get; set; }
        //高压1线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_v_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_e_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd1_transformer_temp_c { get; set; }
        //高压2#线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_v_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_e_c { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double ehvd2_transformer_temp_c { get; set; }
        //电表1#线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_sharp { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_peak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_shoulder { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_active_offpeak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_sharp { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_peak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_shoulder { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_postive_reactive_offpeak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr1_pf_c { get; set; }
        //电表2#线
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_sharp { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_peak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_shoulder { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_active_offpeak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_sharp { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_peak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_shoulder { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_postive_reactive_offpeak { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_all { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_a { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_b { get; set; }
        [Required(ErrorMessage = "该字段不能为空！")]
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double emr2_pf_c { get; set; }
    }
}