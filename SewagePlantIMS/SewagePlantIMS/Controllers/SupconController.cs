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
    [LoginAttribute(isNeed = true)]
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

            SqlCommand cmd;
            SqlDataReader reader;
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
            sql = "select dm_supcon_point.id,device_id,old_point,new_point,indatabase,point_type,describe,title from dm_supcon_point,dm_device where dm_device.id = dm_supcon_point.device_id and dm_device.title like '%" + devicename + "%';";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            string str = "";
            int count = 0;
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                str += "{ \"device_id\": \"" + Convert.ToInt32(reader["device_id"]) + "\", \"describe\": \"" + reader["describe"].ToString() + "\", \"indatabase\": \"" + reader["indatabase"].ToString() + "\", \"new_point\": \"" + reader["new_point"].ToString() + "\", \"old_point\": \"" + reader["old_point"].ToString() + "\",\"point_type\":\"" + reader["point_type"].ToString() + "\",\"id\":\"" + Convert.ToInt32(reader["id"]) + "\",\"title\":\"" + reader["title"].ToString() + "\"},";
                count += 1;
            }
            reader.Close();
            con.Close();

            str = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + str;
            str = str + "]}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public string SupconPointSubmit()
        {
            //添加新的点位
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "insert into dm_supcon_point values(" + Request["device_id"] + ",'" + Request["old_point"] + "','" + Request["new_point"] + "','" + Request["indatabase"] + "','" + Request["point_type"] + "','" + Request["describe"] + "');";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            string str = "";
            if (check == 1)
                str = "{ \"code\": 200, \"msg\": \"操作成功\"}";
            else
                str = "{ \"code\": 200, \"msg\": \"操作失败\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
        public string SupconPointModify()
        {
            //先看看数据是否能够接收
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "update dm_supcon_point set device_id = " + Request["device_id"] + ",describe = '" + Request["describe"] + "',indatabase = '" + Request["indatabase"] + "',new_point='" + Request["new_point"] + "',old_point = '" + Request["old_point"] + "',point_type = '" + Request["point_type"] + "' where id = " + Request["id"];
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            string str = "{ \"code\": 200, \"msg\": \"操作成功\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
            /*
            string str = "{ \"device_id\": \"测试二号\", \"describe\": \"d\", \"indatabase\": \"d\", \"new_point\": \"d\", \"old_point\": \"d\",\"point_type\":\"运行状态\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();*/
        }
        public string DeleteSupconPoint(string id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "delete dm_supcon_point where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            string str = "";
            if (check == 1)
                str = "{ \"code\": 200, \"msg\": \"操作成功\"}";
            else
                str = "{ \"code\": 200, \"msg\": \"操作失败\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }

        /////////////下面是设备状态的内容////////////////////
        public ActionResult DeviceSPList()
        {
            return View();
        }
        public string GetDeviceSPList()
        {
            //连接中控数据库
            //SqlConnection con_supcon = new SqlConnection("server=(local);database=supcon;Integrated Security=True;");
            SqlConnection con_supcon = new SqlConnection("Server=10.10.70.113;DataBase=supcon; User Id=sa; Password=Aa12345678");
            con_supcon.Open();
            if (con_supcon.State != ConnectionState.Open)
            {
                con_supcon.Close();
                string str = "{ \"code\": 200, \"msg\": \"无法正确连接至瓯江口污水厂中控数据库！！！\"}";
                JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                return json.ToString();
            }
            else
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
                con.Open();
                //先查出所有点位的最新信息并建立字典
                /*Dictionary<string, int> PointValue = new Dictionary<string, int>();*/
                //
                string sql = "select dm_supcon_point.id,dm_supcon_point.device_id,dm_device.title,dm_technology.title as name,new_point,indatabase from dm_supcon_point,dm_device,dm_technology where dm_supcon_point.device_id=dm_device.id and dm_device.technology_id = dm_technology.id;";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader reader = cmd.ExecuteReader();
                //预设一个中控数据的查询变量
                string cs_str = "";
                int tt = 0; //判断有无点位数据
                int value = -1;
                string jsonn = "";//返回前端的json字符串
                int count = 0;//总数据条数
                SqlCommand cmd_supcon;
                while (reader.Read())
                {
                    cs_str = "select top 1 val from " + reader["indatabase"].ToString() + " where TagName = '" + reader["new_point"].ToString() + "' order by DateAndTime desc,Millitm desc;";
                    try
                    {
                        cmd_supcon = new SqlCommand(cs_str, con_supcon);
                        value = Convert.ToInt32(cmd_supcon.ExecuteScalar());
                        jsonn += "{ \"id\": \"" + Convert.ToInt32(reader["id"]) + "\", \"device_id\": \"" + Convert.ToInt32(reader["device_id"]) + "\", \"title\": \"" + reader["title"].ToString() + "\", \"name\": \"" + reader["name"].ToString() + "\", \"state\": \"" + value + "\", \"new_point\": \"" + reader["new_point"].ToString() + "\", \"indatabase\": \"" + reader["indatabase"].ToString() + "\"},";
                        count += 1;
                    }
                    catch (Exception e)
                    {

                    }

                    tt += 1;
                }
                reader.Close();
                con_supcon.Close();
                con.Close();
                if (tt == 0)
                {
                    string str = "{ \"code\": 200, \"msg\": \"无点位数据信息\"}";
                    JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                    return json.ToString();
                }
                else
                {
                    jsonn = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + jsonn;
                    jsonn = jsonn + "]}";
                    JObject json = (JObject)JsonConvert.DeserializeObject(jsonn.ToString());
                    return json.ToString();
                }
            }

        }
        public void OutputDeviceRunRecord(string date,string tagname,string indatabase,string title)
        {
            //切割字符串
            string begin_time = date.Substring(0,10);
            string end_time = date.Substring(13,10);
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\DeviceStateRecord.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //往表中插入数据
                //连接中控数据库这里就不判断了，因为既然状态能显示出来就表示连接成功,但是实际上这是会报错的。
                //SqlConnection con_supcon = new SqlConnection("server=(local);database=supcon;Integrated Security=True;");
                SqlConnection con_supcon = new SqlConnection("Server=10.10.70.113;DataBase=supcon; User Id=sa; Password=Aa12345678");
                con_supcon.Open();
                string sql = "select * from " +  indatabase +" where TagName = '"+tagname+" ' and DateAndTime Between '"+begin_time+"' and '"+end_time+"' order by DateAndTime desc,Millitm desc;";
                SqlCommand cmd = new SqlCommand(sql, con_supcon);
                SqlDataReader reader = cmd.ExecuteReader();
                int i = 2;
                ICell Cell;
                //设置单元格样式
                ICellStyle cellstyle = hssfworkbook.CreateCellStyle();
                //设置单元格上下左右边框线
                cellstyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellstyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                while (reader.Read())
                {
                    //第一列
                    Cell = sheet1.CreateRow(i).CreateCell(0);
                    Cell.CellStyle = cellstyle;
                    sheet1.GetRow(i).GetCell(0).SetCellValue(Convert.ToDateTime(reader["DateAndTime"]).ToString("yyyy-MM-dd HH:mm:ss"));
                    //第二列
                    Cell = sheet1.GetRow(i).CreateCell(1);
                    Cell.CellStyle = cellstyle;
                    sheet1.GetRow(i).GetCell(1).SetCellValue(title);
                    //第三列
                    Cell = sheet1.GetRow(i).CreateCell(2);
                    Cell.CellStyle = cellstyle;
                    if(Convert.ToInt32(reader["Val"])==1)
                        sheet1.GetRow(i).GetCell(2).SetCellValue("运行");
                    else
                        sheet1.GetRow(i).GetCell(2).SetCellValue("暂停");
                    i++;
                }
                reader.Close();
                //sheet1.GetRow(1).GetCell(1).SetCellValue(model.repair_class);
                con_supcon.Close();
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, title  + "启停记录");
            }
        }       
        //转到浏览器下载
        public static string DownloadFile(MemoryStream fs, string filename)//必须为FileStream或MemoryStream ，如果用Stream则生成的excel无法正常打开
        {
            string fileName = filename + ".xls";//客户端保存的文件名 //以字符流的形式下载文件 
            byte[] bytes = fs.ToArray(); fs.Read(bytes, 0, bytes.Length); fs.Close();
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";

            //通知浏览器下载文件而不是打开  
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            System.Web.HttpContext.Current.Response.AddHeader("Content-Transfer-Encoding", "binary"); System.Web.HttpContext.Current.Response.BinaryWrite(bytes);
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
            return null;
        }
    }
}
