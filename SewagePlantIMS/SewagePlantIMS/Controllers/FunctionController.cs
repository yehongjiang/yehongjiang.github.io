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
    public class FunctionController : Controller
    {
        // GET: Function
        //Index页就是下载测试表格用没另外命名
        public ActionResult Index()
        {
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\testexcel.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //准备插入图片
                AddCellPicture(sheet1,hssfworkbook, @"G:/yehongjiang.github.io/SewagePlantIMS/SewagePlantIMS/images/DeviceRepairPic3e26c6c4-6159-419f-97be-ae3144466722.png", 0,0);
              
                
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, "测试图片导出表格");
            }
            return View();
        }
        //下载EXCEL文件
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
        //向excel中提交图片
        private void AddCellPicture(ISheet sheet, HSSFWorkbook workbook, string fileurl, int row, int col)
        {
            try
            {
                //由于File类只能读取本地资源，所以在配置文件中配置了物理路径的前半部分
                string DiscPath = ConfigurationManager.AppSettings["PictureDiscPath"];
                // string FileName = DiscPath.Replace("\\", "/") + fileurl.Replace("http://www.bolioptics.com/", "");
                string FileName = fileurl;
                FileInfo file = new FileInfo(FileName);
                if (file.Exists == true)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(FileName);
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        int pictureIdx = workbook.AddPicture(bytes, NPOI.SS.UserModel.PictureType.JPEG);
                        HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
                        HSSFClientAnchor anchor = new HSSFClientAnchor(0, 0, 0, 0, col, row, col + 1, row + 1);
                        HSSFPicture pict = (HSSFPicture)patriarch.CreatePicture(anchor, pictureIdx);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //富文本编辑主页
        public ActionResult ComplexText()
        {
            SqlConnection con = new SqlConnection("server=(local);database=temp;Integrated Security=True;");
            con.Open();
            string sql = "select  top 1 content from richtext order by id desc;";
            SqlCommand cmd = new SqlCommand(sql, con);
            ViewBag.richtext = cmd.ExecuteScalar();
            con.Close();
            return View();
        }
        
        public string RichTextEdit_Submit(string text)
        {
            SqlConnection con = new SqlConnection("server=(local);database=temp;Integrated Security=True;");
            con.Open();
            string sql = "insert into richtext values(1,'" + text + "')";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
            {
                return "已成功返回后台数据" + text;
            }
            else
            {
                return "没有返回后台数据" + text;
            }            
        }
    }
}
    
