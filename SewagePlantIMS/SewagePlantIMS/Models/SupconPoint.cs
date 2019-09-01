using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Models
{
    public class SupconPoint
    {
        public int id  {get;set;}
        public int device_id { get; set; }
        public int old_point { get; set; }
        public string new_point { get; set; }
        public string indatabase {get;set;}
        public string point_type {get;set;}
        public string describe {get;set;}

    }
}