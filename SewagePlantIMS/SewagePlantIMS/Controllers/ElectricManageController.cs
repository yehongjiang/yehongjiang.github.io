using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SewagePlantIMS.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.IO;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;
using System.Drawing;
using SewagePlantIMS.ViewModels;


namespace SewagePlantIMS.Controllers
{

    public class ElectricManageController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ElectricalEquipment()
        {
            return View();
        }
        [HttpGet]
        public ActionResult AddElectrical()
        {
            List<technology> technologys = new List<technology>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,title from dm_technology;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            technology[] tc = new technology[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new technology();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].title = ds.Tables[0].Rows[mDr][1].ToString();

                technologys.Add(tc[mDr]);
            }
            con.Close();
            return View(technologys);
        }
        public ActionResult AddElectrical_Post()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string selectID = "select id from dm_technology where title = '" + Request.Form["technology_name"] + "'";
            SqlCommand cmd2 = new SqlCommand(selectID, con);
            var id = cmd2.ExecuteScalar();
            string sqlStr = "insert into dm_electrical(technology_id,elec_name,remarks,elec_power) values('" + id + "','" + Request.Form["elec_name"] + "','" + Request.Form["remarks"] + "','" + Convert.ToDouble(Request.Form["elec_power"]) + "');";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                Response.Write("<script>alert('添加数据成功！！');window.location.href='Index';</script>");
            else
                Response.Write("<script>alert('添加数据失败！,请手动联系管理员~');</script>");
            return RedirectToAction("Electrical_List");
        }
        [HttpGet]
        public ActionResult Electrical_List()
        {
            List<Electrical> electrical = new List<Electrical>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select * from dm_electrical";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Electrical[] E = new Electrical[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new Electrical();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].elec_name = ds.Tables[0].Rows[mDr][2].ToString();

                electrical.Add(E[mDr]);

                //直接生成二维码图片
                CreateQrCode(E[mDr].id, E[mDr].elec_name);
            }
            return View(electrical);
        }


        public ActionResult DeleteElectrical()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "delete from dm_electrical where id = " + Request.Form["del"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("Electrical_List");
        }

        public ActionResult CreateQrCode(int id, string name)

        {
            string host = Request.Url.Host;
            var port = Request.Url.Port;

            string str = "http://" + host + ":" + port + "/ElectricalManage/ShowElectrical?id=" + id;
            using (var memoryStream = QRCodeHelper.GetQRCode(str, 10))
            {

                System.Drawing.Image img = Image.FromStream(memoryStream);
                string savePath = Server.MapPath("~/QRcode/ElectricQR/" + name + "temp.png");
                img.Save(savePath, ImageFormat.Png);

                AddTextToImg(savePath, name, name);
                img.Dispose();
                //img = Image.FromFile("~/QRcode/ElectricQR/" + name + ".png");
                //下面这段是将image转换为流从而输出到页面的img标签上去
                /*MemoryStream ms = new MemoryStream();
                byte[] imagedata = null;
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imagedata = ms.GetBuffer();
                Response.ContentType = "image/jpeg";
                Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                Response.End();*/
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

            float rectY = 390;
            float rectX = rectY / 2 - rectY / 4;
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

            //输出方法一：将文件生成并保存到对应文件夹
            string path = Server.MapPath("~/QRcode/ElectricQR/" + picname + ".png");
            Image img = bitmap;
            img.Save(path, ImageFormat.Png);

            //bitmap.Save(path, ImageFormat.Png);

            //输出方法二，显示在网页中，保存为Jpg类型
            //bitmap.Save(ms, ImageFormat.Jpeg);
            //Response.Clear();
            //Response.ContentType = "image/jpeg";
            //Response.BinaryWrite(ms.ToArray());
            g.Dispose();
            bitmap.Dispose();
            image.Dispose();
            img.Dispose();
            //删除带temp的二维码图片
            path = Server.MapPath("~/QRcode/ElectricQR/" + picname + "temp.png");
            System.IO.File.Delete(path);
        }

        public ActionResult ShowElectrical()
        {
            List<ShowElectrical> electrical = new List<ShowElectrical>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select dm_electrical.id,title,elec_name,remarks,elec_power,pic_url from dm_electrical,dm_technology,dm_elec_pic where dm_electrical.id = '"+ Request.Form["show"] +"'and technology_id = dm_technology.id and dm_electrical.id = dm_elec_pic.elec_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ShowElectrical[] E = new ShowElectrical[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new ShowElectrical();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].technology_name = ds.Tables[0].Rows[mDr][1].ToString();
                E[mDr].elec_name = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].remarks = ds.Tables[0].Rows[mDr][3].ToString();
                E[mDr].elec_power = ds.Tables[0].Rows[mDr][4].ToString();
                E[mDr].pic_url = ds.Tables[0].Rows[mDr][5].ToString();

                electrical.Add(E[mDr]);
            }
            return View(electrical);
        }
        //前端下载二维码图片
        public void DownloadQRcode()
        {
            string path = "~/QRcode/ElectricQR/" + Request.Form["qrcode"] + ".png";
            RenderToBrowser(path, Request.Form["qrcode"]);
        }
        public void RenderToBrowser(string filePath, string picname)
        {
            filePath = Server.MapPath(filePath);//路径 
            //以字符流的形式下载文件 
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            Response.ContentType = "application/octet-stream";
            //文件名+文件格式 （这里编码采用的是utf-8）
            Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(picname + ".png", System.Text.Encoding.UTF8));
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }

        [HttpGet]
        public ViewResult ElectricalPic(int id)
        {
            ViewBag.elec_id = id;
            List<elec_pic> elec_pics = new List<elec_pic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,pic_url from dm_elec_pic where elec_id = " + ViewBag.elec_id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            elec_pic[] tc = new elec_pic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new elec_pic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][1].ToString();

                elec_pics.Add(tc[mDr]);
            }
            con.Close();
            return View(elec_pics);
        }
        //添加设备图片
        [HttpPost]
        public ViewResult ElectricalPic()
        {
            ViewBag.elec_id = Request.Form["id"];
            List<elec_pic> elec_pics = new List<elec_pic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,pic_url from dm_elec_pic where elec_id = " + ViewBag.elec_id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            elec_pic[] tc = new elec_pic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new elec_pic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][1].ToString();

                elec_pics.Add(tc[mDr]);
            }
            con.Close();
            return View(elec_pics);
        }
        public void ElectricalPicPost()
        {
            elec_pic ep = new elec_pic();
            HttpPostedFileBase ff = Request.Files["File"];

            /*if (ff != null && ff.ContentLength != 0)
             {
                 if (!Directory.Exists(Server.MapPath("~images/ElectricalPic")))
                 {
                     Directory.CreateDirectory("~images/ElectricalPic");
                 }
             }*/
            //修改文件名去掉其原有的扩展名
            string filename = Request.Form["filename2"];
            string[] filename_temp = filename.Split('.');
            filename = filename_temp[0];
            string filepath = Server.MapPath("/images/ElectricalPic/" + filename + "_"+ Request.Form["id"] + ".png");
            ff.SaveAs(filepath);//在服务器上保存上传文件
                                //string[] readFile = System.IO.File.ReadAllLines(filepath);//读取txt文档存放在字符数组中

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "insert into dm_elec_pic (pic_name,pic_url,add_date,elec_id) values('" + filename + "','" + "/images/ElectricalPic/" + filename + "_" + Request.Form["id"] + ".png" + "','" + DateTime.Now.ToString() + "','" + Request.Form["id"] + "')";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            if (check == 1)
            {
                Response.Write("<script>alert('添加图片成功！！');window.location.href='ElectricalPic?id=" + Request.Form["id"] + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('添加图片失败！,请手动联系管理员~');</script>");
            con.Close();
            //return RedirectToAction("Electrical_List"); //如果直接View就不会执行HttpGet的代码
            //return ElectricalPic();
        }
        //删除对应的图片
        public void DeletePic()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先查出url
            string pic_url = "select pic_url from dm_elec_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd_url = new SqlCommand(pic_url, con);
            pic_url = cmd_url.ExecuteScalar().ToString();
            string sql = "select elec_id from dm_elec_pic  where id =  '"+Request.Form["del_id"]+"'";
            SqlCommand cmd = new SqlCommand(sql, con);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            string sqlStr = "delete from dm_elec_pic where id =  '"+Request.Form["del_id"]+"'";
            SqlCommand cmd2 = new SqlCommand(sqlStr, con);
            int check = cmd2.ExecuteNonQuery();
            con.Close();
            //删除对应的实体文件
            string filePath = Server.MapPath(pic_url);//路径 
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
                
            if (check == 1)
            {
                Response.Write("<script>alert('删除图片成功！！');window.location.href='ElectricalPic?id=" + id + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('删除失败！,请手动联系管理员~');</script>");

        }
    }

}

