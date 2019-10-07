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
            //在第一次页面加载的时候给前端返回一个字典包含了设备ID和TITLE
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //建立设备id与对应名称的字典
            string sql = "select id,title from dm_device;";
            Dictionary<int, string> dic_device_id_title = new Dictionary<int, string>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                dic_device_id_title[Convert.ToInt32(reader["id"])] = reader["title"].ToString();
            }
            reader.Close();
            con.Close();
            ViewBag.DeviceName = dic_device_id_title;
            return View();
        }
        public string SupconPointListGet()
        {
            //查出数据库dm_supcon_point中的数据
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();

            string sql = "";

            SqlCommand cmd ;
            SqlDataReader reader ;
            //将sql变成查询中控表的语句
            //看看Request[devicename]是不是为空
            string devicename = "";
            if (Request["devicename"] == null)
            {
                devicename = "";
            }
            else
            {
                devicename = Request["devicename"].ToString();
            }
            sql = "select dm_supcon_point.id,device_id,old_point,new_point,indatabase,point_type,describe,title from dm_supcon_point,dm_device where dm_device.id = dm_supcon_point.device_id and dm_device.title like '%" + devicename +"%';";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            string str = "";
            int count = 0;
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                str += "{ \"device_id\": \"" + Convert.ToInt32(reader["device_id"]) + "\", \"describe\": \"" + reader["describe"].ToString() +"\", \"indatabase\": \""+reader["indatabase"].ToString()+"\", \"new_point\": \""+reader["new_point"].ToString()+"\", \"old_point\": \""+reader["old_point"].ToString()+"\",\"point_type\":\""+reader["point_type"].ToString()+ "\",\"id\":\"" + Convert.ToInt32(reader["id"]) + "\",\"title\":\"" + reader["title"].ToString() + "\"},";
                count += 1;
            }
            reader.Close();
            con.Close();
           
            str = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + str;
            str = str + "]}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public string SupconPointSubmit(Object data)
        {
            
            string str = "{ \"code\": 200, \"msg\": \"操作成功\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public void SupconPointModify()
        {
            //先看看数据是否能够接收
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "update dm_supcon_point set device_id = " + Request["device_id"] + "where id = " + Request["id"];
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            /*
            string str = "{ \"device_id\": \"测试二号\", \"describe\": \"d\", \"indatabase\": \"d\", \"new_point\": \"d\", \"old_point\": \"d\",\"point_type\":\"运行状态\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();*/
        }
    }
}