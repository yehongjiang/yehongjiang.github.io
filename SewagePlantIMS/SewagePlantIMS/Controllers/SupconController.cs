using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SewagePlantIMS.Filter;
using SewagePlantIMS.Function;
using SewagePlantIMS.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Drawing;
//为了将SQL数据转为JSON数据而引用的
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
//Excel操作
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace SewagePlantIMS.Controllers
{
    public class SupconController : Controller
    {
        // GET: Supcon
        public ActionResult SupconPointList()
        {
            List<string> temp = new List<string>();
            temp.Add("测试一号");
            temp.Add("测试二号");
            ViewBag.DeviceName = temp;
            return View();
        }
        public string SupconPointListGet()
        {
            string str = "{ \"DeivceId\": \"测试二号\", \"describe\": \"d\", \"indatabase\": \"d\", \"new_point\": \"d\", \"old_point\": \"d\",\"point_type\":\"运行状态\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public string SupconPointSubmit(Object data)
        {
            
            string str = "{ \"code\": 200, \"msg\": \"操作成功\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public string SupconPointModify()
        {
            string str = "{ \"DeivceId\": \"测试二号\", \"describe\": \"d\", \"indatabase\": \"d\", \"new_point\": \"d\", \"old_point\": \"d\",\"point_type\":\"运行状态\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
    }
}