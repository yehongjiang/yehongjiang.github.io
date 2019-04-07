using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tryadonet.Models;
//数据库操作需要用到以下命名空间
using System.Data;
using System.Data.SqlClient;
//哈希加密需要用到以下命名空间
using System.Web.Security;
using System.Configuration;

using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;


using System.Drawing;
using System.Drawing.Imaging;
//using System.IO;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;

using ThoughtWorks.QRCode.Codec;
using System.Web.Script.Serialization;
using System.Text;
using tryadonet.ViewModel;


namespace tryadonet.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {

            List<Student> students = new List<Student>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            string sqlStr = "select * from student_info;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Student[] S = new Student[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                S[mDr] = new Student();
                S[mDr].stu_id = ds.Tables[0].Rows[mDr][0].ToString();
                S[mDr].stu_name = ds.Tables[0].Rows[mDr][1].ToString();
                S[mDr].stu_sex = ds.Tables[0].Rows[mDr][2].ToString();
                S[mDr].stu_age = ds.Tables[0].Rows[mDr][3].ToString();
                S[mDr].stu_dept = ds.Tables[0].Rows[mDr][4].ToString();
                students.Add(S[mDr]);
            }

            return View(students);
        }
        [HttpGet]
        public ActionResult AddStudent()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddStudent(Student student)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            con.Open();
            string sqlStr = "insert into student_info values('" + Request.Form["ID"] + "','" + Request.Form["Name"] + "','" + Request.Form["Sex"] + "','" + Request.Form["Age"] + "','" + Request.Form["Dept"] + "');";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                Response.Write("<script>alert('添加数据成功！！');window.location.href='Index';</script>");
            else
                Response.Write("<script>alert('添加数据失败！');</script>");

            return View();
        }
        [HttpPost]
        public ActionResult DeleteInfo(string stu_id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            con.Open();
            string sqlStr = "Delete from student_info where stu_id = " + Request.Form["del"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("Index");

        }
        [HttpPost]
        public ActionResult OutTable(string stu_id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            string sqlStr = "select * from student_info where stu_id = " + "'" + Request.Form["output"] + "'" + ";";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);

            /* HSSFWorkbook hssfworkbook = new HSSFWorkbook(); //创建一个Excel
             ISheet sheet1 = hssfworkbook.CreateSheet("Sheet1");//创建表格
             hssfworkbook.CreateSheet("Sheet2");
             hssfworkbook.CreateSheet("Sheet3");
             IRow row = sheet1.CreateRow(0);     //  创建第一行
             ICell cell = row.CreateCell(0); //创建第一个单元格
             cell.SetCellValue(1.225);//往第一个单元格里输入内容
             //这里注意应该也不用创建相应的实例而批量构建数据

              如果你觉得每一行要声明一个HSSFRow很麻烦，可以用下面的方式：
             sheet1.CreateRow(0).CreateCell(0).SetCellValue("This is a Sample");
             这么用有个前提，那就是第0行还没创建过，否则得这么用：
             sheet1.GetRow(0).CreateCell(0).SetCellValue("This is a Sample");

            sheet1.SetColumnWidth(0, 20 * 256);
             sheet1.SetColumnWidth(1, 20 * 256);
            // row.Height = 20 * 20;//行高设置
             //cell.CellStyle = HSSFCellStyle.ALIGN_CENTER;
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
             cellStyle.Alignment = HorizontalAlignment.Center;
             row.GetCell(0).CellStyle = cellStyle;
             */
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            //打开模板文件到文件流中
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"Excel\template.xls", FileMode.Open, FileAccess.Read))
            {
                //将文件流中模板加载到工作簿对象中
                hssfworkbook = new HSSFWorkbook(file);
            }
            ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
            sheet1.GetRow(1).GetCell(0).SetCellValue(ds.Tables[0].Rows[0][0].ToString());
            sheet1.GetRow(1).GetCell(1).SetCellValue(ds.Tables[0].Rows[0][1].ToString());
            sheet1.GetRow(1).GetCell(2).SetCellValue(ds.Tables[0].Rows[0][2].ToString());
            sheet1.GetRow(1).GetCell(3).SetCellValue(ds.Tables[0].Rows[0][3].ToString());
            sheet1.GetRow(1).GetCell(4).SetCellValue(ds.Tables[0].Rows[0][4].ToString());

            MemoryStream mstream = new MemoryStream();
            hssfworkbook.Write(mstream);
            DownloadFile(mstream, ds.Tables[0].Rows[0][1].ToString());


            return RedirectToAction("Index");
        }
        //在客户端保存或查看用流生成的excel文件
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

        public ActionResult tag()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Detail()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            string sqlStr = "select * from student_info where stu_id = " + "'" + Request.Form["output"] + "'" + ";";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Student s = new Student();
            s.stu_id = ds.Tables[0].Rows[0][0].ToString();
            s.stu_name = ds.Tables[0].Rows[0][1].ToString();
            s.stu_sex = ds.Tables[0].Rows[0][2].ToString();
            s.stu_age = ds.Tables[0].Rows[0][3].ToString();
            s.stu_dept = ds.Tables[0].Rows[0][4].ToString();
            return View(s);
        }
        [HttpGet]
        public ActionResult Detail(string id)
        {
            ViewData["ID"] = id;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["tryadonet"].ConnectionString);
            string sqlStr = "select * from student_info where stu_id = " + "'" + id + "'" + ";";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Student s = new Student();
            s.stu_id = ds.Tables[0].Rows[0][0].ToString();
            s.stu_name = ds.Tables[0].Rows[0][1].ToString();
            s.stu_sex = ds.Tables[0].Rows[0][2].ToString();
            s.stu_age = ds.Tables[0].Rows[0][3].ToString();
            s.stu_dept = ds.Tables[0].Rows[0][4].ToString();
            return View(s);
        }
        public ActionResult QRCoder()
        {
            return View();
        }

        public ActionResult CreateQrCode()

        {
            string host = Request.Url.Host;
            var port = Request.Url.Port;

            string str = "http://" + host + ":" + port + "/Home/Detail?id=100";
            using (var memoryStream = QRCodeHelper.GetQRCode(str, 10))
            {
                /*Response.ContentType = "image/jpeg";
                Response.OutputStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                Response.End();*/
                Image img = Image.FromStream(memoryStream);
                //Graphics graphics = Graphics.FromImage(img);
                string savePath = "C:/Users/11619/Desktop/dsds/123.png";
                img.Save(savePath, ImageFormat.Png);

                AddTextToImg("C:/Users/11619/Desktop/dsds/123.png", "鼓风机房配电柜", "yehongjiang");
                img.Dispose();
                img = Image.FromFile("C:/Users/11619/Desktop/dsds/567.png");
                //下面这段是将image转换为流从而输出到页面的img标签上去
                MemoryStream ms = new MemoryStream();
                byte[] imagedata = null;
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imagedata = ms.GetBuffer();
                Response.ContentType = "image/jpeg";
                Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                Response.End();
            }
            return null;
        }



        public class QRCodeHelper

        {

            /// <summary>  

            /// 生成二维码  

            /// </summary>  

            /// <param name="content">内容</param>

            /// <param name="moduleSize">二维码的大小</param>

            /// <returns>输出流</returns>  

            public static MemoryStream GetQRCode(string content, int moduleSize)

            {

                //ErrorCorrectionLevel 误差校正水平

                //QuietZoneModules     空白区域



                var encoder = new QrEncoder(ErrorCorrectionLevel.M);

                QrCode qrCode = encoder.Encode(content);

                GraphicsRenderer render = new GraphicsRenderer(new FixedModuleSize(moduleSize, QuietZoneModules.Two), Brushes.Black, Brushes.White);



                MemoryStream memoryStream = new MemoryStream();

                render.WriteToStream(qrCode.Matrix, ImageFormat.Jpeg, memoryStream);



                return memoryStream;



                //生成图片的代码

                //DrawingSize dSize = render.SizeCalculator.GetSize(qrCode.Matrix.Width);

                //Bitmap map = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);

                //Graphics g = Graphics.FromImage(map);

                //render.Draw(g, qrCode.Matrix);

                //map.Save(fileName, ImageFormat.Jpeg);//fileName为存放的图片路径

            }



            /// <summary>

            /// 生成带Logo二维码  

            /// </summary>

            /// <param name="content">内容</param>

            /// <param name="iconPath">logo路径</param>

            /// <param name="moduleSize">二维码的大小</param>

            /// <returns>输出流</returns>

            public static MemoryStream GetQRCode(string content, string iconPath, int moduleSize = 9)

            {

                QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);

                QrCode qrCode = qrEncoder.Encode(content);



                GraphicsRenderer render = new GraphicsRenderer(new FixedModuleSize(moduleSize, QuietZoneModules.Two), Brushes.Black, Brushes.White);



                DrawingSize dSize = render.SizeCalculator.GetSize(qrCode.Matrix.Width);

                Bitmap map = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);

                Graphics g = Graphics.FromImage(map);

                render.Draw(g, qrCode.Matrix);



                //追加Logo图片 ,注意控制Logo图片大小和二维码大小的比例

                //PS:追加的图片过大超过二维码的容错率会导致信息丢失,无法被识别

                Image img = Image.FromFile(iconPath);



                Point imgPoint = new Point((map.Width - img.Width) / 2, (map.Height - img.Height) / 2);

                g.DrawImage(img, imgPoint.X, imgPoint.Y, img.Width, img.Height);



                MemoryStream memoryStream = new MemoryStream();

                map.Save(memoryStream, ImageFormat.Jpeg);
                string fileName = "~/image/";
                map.Save(fileName, ImageFormat.Jpeg);

                return memoryStream;



                //生成图片的代码： map.Save(fileName, ImageFormat.Jpeg);//fileName为存放的图片路径

            }

        }

        //在图片上添加文字
        private void AddTextToImg(string fileName, string text, string picname)
        {

            //判断指定图片是否存在
            /* if (!File.Exists(Server.MapPath(fileName)))
             {
                 throw new FileNotFoundException("The file don't exist!");
             }*/
            if (text == string.Empty)
            {
                return;
            }

            System.Drawing.Image image = System.Drawing.Image.FromFile(fileName);
            Bitmap bitmap = new Bitmap(image, image.Width, image.Height);
            Graphics g = Graphics.FromImage(bitmap);
            float fontSize = 10.0f;             //字体大小
            float textWidth = text.Length * fontSize;  //文本的长度
                                                       //下面定义一个矩形区域，以后在这个矩形里画上白底黑字

            float rectY = 310;
            float rectX = rectY / 3;
            float rectWidth = text.Length * (fontSize + 40);
            float rectHeight = fontSize + 40;
            //声明矩形域
            RectangleF textArea = new RectangleF(rectX, rectY, rectWidth, rectHeight);
            Font font = new Font("微软雅黑", fontSize, FontStyle.Bold);   //定义字体
                                                                      //font.Bold = true;
            Brush whiteBrush = new SolidBrush(Color.Black);   //白笔刷，画文字用
                                                              //Brush blackBrush = new SolidBrush(Color.Black);   //黑笔刷，画背景用
                                                              //g.FillRectangle(blackBrush, rectX, rectY, rectWidth, rectHeight);
            g.DrawString(text, font, whiteBrush, textArea);
            MemoryStream ms = new MemoryStream();
            //输出方法一：将文件生成并保存到C盘
            string path = "C:/Users/11619/Desktop/dsds/567.png";
            bitmap.Save(path, ImageFormat.Png);

            //输出方法二，显示在网页中，保存为Jpg类型
            //bitmap.Save(ms, ImageFormat.Jpeg);
            //Response.Clear();
            //Response.ContentType = "image/jpeg";
            //Response.BinaryWrite(ms.ToArray());
            g.Dispose();
            bitmap.Dispose();
            image.Dispose();
        }
        public ActionResult Ajax()
        {
            List<int> list = new List<int> { 1, 2, 3, 4 };
            ViewBag.list = list;
            return View();
        }
        [HttpPost]
        public JavaScriptResult Ajax(int id, string name)
        {
            return JavaScript("haha()");


        }

        public ActionResult UpdateHtml()
        {
            return View();
        }
        //分部页面

        [HttpGet]
        public PartialViewResult AllStudent()
        {


            temp ss = new temp();
            ss.FirstName = "叶泓江";
            ss.LastName = "hehe";
            ss.MobileNo = "22222";
            ss.Address = "上楼";

            var objAllStudent = ss;
            return PartialView("AllStudent", objAllStudent);
        }

        public ActionResult motaikuang()
        {
            return View();
        }
        public JavaScriptResult temp()
        {
            return JavaScript("swal_success();jump();");
        }
    }
}