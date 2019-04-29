using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace tryadonet.Models
{
    public class Student
    {
        public string stu_id { get; set; }
        [Display(Name = "学生姓名")]
        [Required(ErrorMessage = "学生名不能为空哦~")]
        public string stu_name { get; set; }
        public string stu_sex { get; set; }
        [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "输入的数据格式不正确！")]
        public double stu_age { get; set; }
        public string stu_dept { get; set; }
    }
}