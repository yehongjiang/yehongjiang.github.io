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
        public string stu_name { get; set; }
        public string stu_sex { get; set; }

        public string stu_age { get; set; }
        public string stu_dept { get; set; }
    }
}