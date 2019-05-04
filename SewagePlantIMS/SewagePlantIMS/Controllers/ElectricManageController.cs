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
using SewagePlantIMS.Function;
using SewagePlantIMS.Filter;
using System.Web.UI;

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
    public class ElectricManageController : Controller
    {
        /// <summary>
        /// 下面是仪表二维码的内容和方法
        /// </summary>
        /// <returns></returns>
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
            con.Open();
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
        public JavaScriptResult AddElectrical_Post()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string selectID = "select id from dm_technology where title = '" + Request.Form["technology_name"] + "'";
            SqlCommand cmd2 = new SqlCommand(selectID, con);
            var id = cmd2.ExecuteScalar();
            string sqlStr = "insert into dm_electrical(technology_id,elec_name,remarks,in_port,out_port,purpose) values('" + id + "','" + Request.Form["elec_name"] + "','" + Request.Form["remarks"] + "','" + Request.Form["in_port"] + "','" + Request.Form["out_port"] + "','" + Request.Form["purpose"] + "');";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                return JavaScript("swal_success();jump_electrical_list();");
            else
                return JavaScript("swal_error();");
        }
        [HttpGet]
        public ActionResult Electrical_List()
        {
            List<Electrical> electrical = new List<Electrical>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select id,elec_name from dm_electrical";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Electrical[] E = new Electrical[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new Electrical();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].elec_name = ds.Tables[0].Rows[mDr][1].ToString();

                electrical.Add(E[mDr]);

                //直接生成二维码图片
                CreateQrCode(E[mDr].id, E[mDr].elec_name);
            }
            con.Close();
            return View(electrical);
        }


        public ActionResult DeleteElectrical()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先删除对应的图片
            string del_pic = "select pic_url from dm_elec_pic where elec_id = " + Request.Form["del"];
            SqlDataAdapter da = new SqlDataAdapter(del_pic, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {

                string filePath = Server.MapPath(ds.Tables[0].Rows[mDr][0].ToString());
                FileInfo file2 = new FileInfo(filePath);
                if (file2.Exists)
                {
                    file2.Delete();
                }
            }
            //删除对应的二维码文件
            string del_qrcode = "select qrcode from dm_electrical where id = " + Request.Form["del"];
            SqlCommand cmd1 = new SqlCommand(del_qrcode, con);
            string url = Server.MapPath(cmd1.ExecuteScalar().ToString());
            FileInfo file = new FileInfo(url);
            file.Delete();

            //最后删除相应的数据库数据
            string sqlStr = "delete from dm_elec_pic where elec_id = " + Request.Form["del"] + "; delete from dm_electrical where id = " + Request.Form["del"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("Electrical_List");
        }
        public ActionResult CreateQrCode(int id, string name)

        {
            string host = Request.Url.Host;
            var port = Request.Url.Port;

            string str = "http://" + host + ":" + port + "/ElectricManage/ShowElectrical?id=" + id;

            //向数据库中存入二维码图片地址
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();

            using (var memoryStream = QRCodeHelper.GetQRCode(str, 10))
            {

                System.Drawing.Image img = Image.FromStream(memoryStream);
                string savePath = Server.MapPath("~/QRcode/ElectricQR/" + name + "temp.png");
                img.Save(savePath, ImageFormat.Png);

                AddTextToImg(savePath, name, name);
                img.Dispose();

                string sql = "update dm_electrical set qrcode = '" + "~/QRcode/ElectricQR/" + name + ".png' where id =" + id + ";";
                SqlCommand cmd = new SqlCommand(sql, con);
                int check = cmd.ExecuteNonQuery();
                con.Close();
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
            con.Close();
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
            float rectX = 15;
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
        [LoginAttribute(isNeed = false)]
        [HttpGet]
        public ActionResult ShowElectrical(string id)
        {
            List<ShowElectrical> electrical = new List<ShowElectrical>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select dm_electrical.id,title,elec_name,remarks,in_port,pic_url,out_port,purpose from dm_electrical,dm_technology,dm_elec_pic where dm_electrical.id = '" + id + "'and technology_id = dm_technology.id and dm_electrical.id = dm_elec_pic.elec_id; ";
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
                E[mDr].in_port = ds.Tables[0].Rows[mDr][4].ToString();
                E[mDr].pic_url = ds.Tables[0].Rows[mDr][5].ToString();
                E[mDr].out_port = ds.Tables[0].Rows[mDr][6].ToString();
                E[mDr].purpose = ds.Tables[0].Rows[mDr][7].ToString();

                electrical.Add(E[mDr]);
            }
            con.Close();
            return View(electrical);
        }
        [HttpPost]
        public ActionResult ShowElectrical()
        {
            List<ShowElectrical> electrical = new List<ShowElectrical>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select dm_electrical.id,title,elec_name,remarks,in_port,pic_url,out_port,purpose from dm_electrical,dm_technology,dm_elec_pic where dm_electrical.id = '" + Request.Form["show"] + "'and technology_id = dm_technology.id and dm_electrical.id = dm_elec_pic.elec_id; ";
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
                E[mDr].in_port = ds.Tables[0].Rows[mDr][4].ToString();
                E[mDr].pic_url = ds.Tables[0].Rows[mDr][5].ToString();
                E[mDr].out_port = ds.Tables[0].Rows[mDr][6].ToString();
                E[mDr].purpose = ds.Tables[0].Rows[mDr][7].ToString();

                electrical.Add(E[mDr]);
            }
            con.Close();
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

        [HttpPost]
        public ViewResult ElectricalPic()
        {
            ViewBag.elec_id = Request.Form["id"];
            List<elec_pic> elec_pics = new List<elec_pic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
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
            //读取elec_pic表最后一个id号加1最为不重复图片名的依据
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select max(id) from dm_elec_pic";
            SqlCommand cmd1 = new SqlCommand(sql, con);
            int max_id;
            if (cmd1.ExecuteScalar() != DBNull.Value)
            {
                max_id = Convert.ToInt32(cmd1.ExecuteScalar());
                max_id++;
            }
            else
            {
                max_id = 1;
            }
            string filepath = Server.MapPath("/images/ElectricalPic_Pre/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            ff.SaveAs(filepath);//在服务器上保存上传原图文件
                                //string[] readFile = System.IO.File.ReadAllLines(filepath);//读取txt文档存放在字符数组中
            string filepath2 = Server.MapPath("/images/ElectricalPic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            CompressPic cp = new CompressPic();
            bool temp = cp.CompressImage(filepath, filepath2, 80, 150, true); //保存压缩完的文件到ElectricalPic文件夹


            string sqlStr = "insert into dm_elec_pic (pic_name,pic_url,add_date,elec_id) values('" + filename + "','" + "/images/ElectricalPic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png" + "','" + DateTime.Now.ToString() + "','" + Request.Form["id"] + "')";
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
            string sql = "select elec_id from dm_elec_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd = new SqlCommand(sql, con);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            string sqlStr = "delete from dm_elec_pic where id =  '" + Request.Form["del_id"] + "'";
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

        [HttpPost]
        public ActionResult ModifyElectrical()
        {
            //先把所有的工艺段名称和ID都找出来放到ViewBag.List
            List<string> technologys = new List<string>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select title from dm_technology;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            technology tc = new technology();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc.title = ds.Tables[0].Rows[mDr][0].ToString();
                technologys.Add(tc.title);
            }
            ViewBag.List = technologys;
            //然后把修改的部分查出来
            sqlStr = "select elec_name,dm_technology.title,in_port,out_port,purpose,remarks from dm_electrical,dm_technology where dm_technology.id = dm_electrical.technology_id and dm_electrical.id =" + Request.Form["id"];
            da = new SqlDataAdapter(sqlStr, con);
            DataSet ds2 = new DataSet();
            da.Fill(ds2);
            Electrical elec = new Electrical();
            elec.elec_name = ds2.Tables[0].Rows[0][0].ToString();
            elec.technology_name = ds2.Tables[0].Rows[0][1].ToString();
            elec.in_port = ds2.Tables[0].Rows[0][2].ToString();
            elec.out_port = ds2.Tables[0].Rows[0][3].ToString();
            elec.purpose = ds2.Tables[0].Rows[0][4].ToString();
            elec.remarks = ds2.Tables[0].Rows[0][5].ToString();
            con.Close();
            return View(elec);
        }
        public JavaScriptResult ModifyElectricl_Post()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string selectID = "select id from dm_technology where title = '" + Request.Form["technology_name"] + "'";
            SqlCommand cmd2 = new SqlCommand(selectID, con);
            var id = cmd2.ExecuteScalar();
            selectID = "select id from dm_electrical where elec_name = '" + Request.Form["elec_name"] + "'";
            cmd2 = new SqlCommand(selectID, con);
            var elec_id = cmd2.ExecuteScalar();
            string sqlStr = "update dm_electrical set technology_id = '" + id + "' where id = '" + elec_id + "';" +
                            "update dm_electrical set elec_name = '" + Request.Form["elec_name"] + "' where id = '" + elec_id + "';" +
                            "update dm_electrical set remarks = '" + Request.Form["remarks"] + "' where id = '" + elec_id + "';" +
                            "update dm_electrical set in_port = '" + Request.Form["in_port"] + "' where id = '" + elec_id + "';" +
                            "update dm_electrical set out_port = '" + Request.Form["out_port"] + "' where id = '" + elec_id + "';" +
                            "update dm_electrical set purpose = '" + Request.Form["purpose"] + "' where id = '" + elec_id + "';";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
                return JavaScript("swal_success();jump_electrical_list();");
            else
                return JavaScript("swal_error();");
        }


        /// <summary>
        /// 下面是电表抄度的内容和方法
        /// </summary>
        /// <returns></returns>

        public ActionResult ElecReadingList()
        {
            List<ElectricReading> electrical = new List<ElectricReading>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select id,add_time,user_name,remark from dm_electric_inspection order by add_time desc;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ElectricReading[] E = new ElectricReading[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new ElectricReading();
                E[mDr].total_id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].add_time = Convert.ToDateTime(ds.Tables[0].Rows[mDr][1]);
                E[mDr].user_name = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].remark = ds.Tables[0].Rows[mDr][3].ToString();
                electrical.Add(E[mDr]);
            }
            con.Close();
            return View(electrical);
        }
        public ActionResult AddElecReading()
        {
            return View();
        }
        public JavaScriptResult BeginElecReading(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "insert into dm_electric_inspection (add_time, user_id, user_name,remark)values('" + Request.Form["aer_check_time"] + "'," + Session["user_id"] + ", '" + Session["real_name"] + "','" + Request.Form["aer_remark"] + "');select @@IDENTITY;";
            SqlCommand cmd = new SqlCommand(select, con);
            var pre_id = Convert.ToInt32(cmd.ExecuteScalar());
            //插入其他表
            string select2 = "insert into dm_elec_inspection_info(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_low_voltage_distributor_1(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_low_voltage_distributor_2(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_low_voltage_switcher(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_high_voltage_distributor_1(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_high_voltage_distributor_2(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_meter_reading_1(electric_id) values(" + pre_id + ");" +
                             "insert into dm_elec_meter_reading_2(electric_id) values(" + pre_id + ");";
            cmd = new SqlCommand(select2, con);
            var check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (pre_id > 0 && check == 8)
                return JavaScript("swal_success();GetPreId(" + pre_id + ");jump_to_db1x()");
            else
                return JavaScript("swal_error();");
        }
        //插入电表1#线的数据
        public JavaScriptResult Insert_db1x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_meter_reading_1 set postive_active_all = " + Model.emr1_postive_active_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_active_sharp = " + Model.emr1_postive_active_sharp + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_active_peak = " + Model.emr1_postive_active_peak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_active_shoulder = " + Model.emr1_postive_active_shoulder + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_active_offpeak = " + Model.emr1_postive_active_offpeak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_reactive_all = " + Model.emr1_postive_reactive_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_reactive_sharp = " + Model.emr1_postive_reactive_sharp + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_reactive_peak = " + Model.emr1_postive_reactive_peak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_reactive_shoulder = " + Model.emr1_postive_reactive_shoulder + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set postive_reactive_offpeak = " + Model.emr1_postive_reactive_offpeak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set pf_all = " + Model.emr1_pf_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set pf_a = " + Model.emr1_pf_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set pf_b = " + Model.emr1_pf_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_1 set pf_c = " + Model.emr1_pf_c + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_db2x()");
            else
                return JavaScript("swal_error();");
        }
        //插入电表2#线的数据
        public JavaScriptResult Insert_db2x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_meter_reading_2 set postive_active_all = " + Model.emr2_postive_active_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_active_sharp = " + Model.emr2_postive_active_sharp + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_active_peak = " + Model.emr2_postive_active_peak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_active_shoulder = " + Model.emr2_postive_active_shoulder + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_active_offpeak = " + Model.emr2_postive_active_offpeak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_reactive_all = " + Model.emr2_postive_reactive_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_reactive_sharp = " + Model.emr2_postive_reactive_sharp + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_reactive_peak = " + Model.emr2_postive_reactive_peak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_reactive_shoulder = " + Model.emr2_postive_reactive_shoulder + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set postive_reactive_offpeak = " + Model.emr2_postive_reactive_offpeak + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set pf_all = " + Model.emr2_pf_all + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set pf_a = " + Model.emr2_pf_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set pf_b = " + Model.emr2_pf_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_meter_reading_2 set pf_c = " + Model.emr2_pf_c + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_gy1x()");
            else
                return JavaScript("swal_error();");
        }
        //插入高压1#线的数据
        public JavaScriptResult Insert_gy1x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_high_voltage_distributor_1 set v_a = " + Model.ehvd1_v_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set v_b = " + Model.ehvd1_v_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set v_c = " + Model.ehvd1_v_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set e_a = " + Model.ehvd1_e_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set e_b = " + Model.ehvd1_e_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set e_c = " + Model.ehvd1_e_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set transformer_temp_a = " + Model.ehvd1_transformer_temp_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set transformer_temp_b = " + Model.ehvd1_transformer_temp_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_1 set transformer_temp_c = " + Model.ehvd1_transformer_temp_c + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_gy2x()");
            else
                return JavaScript("swal_error();");
        }
        //插入高压2#线的数据
        public JavaScriptResult Insert_gy2x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_high_voltage_distributor_2 set v_a = " + Model.ehvd2_v_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set v_b = " + Model.ehvd2_v_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set v_c = " + Model.ehvd2_v_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set e_a = " + Model.ehvd2_e_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set e_b = " + Model.ehvd2_e_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set e_c = " + Model.ehvd2_e_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set transformer_temp_a = " + Model.ehvd2_transformer_temp_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set transformer_temp_b = " + Model.ehvd2_transformer_temp_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_high_voltage_distributor_2 set transformer_temp_c = " + Model.ehvd2_transformer_temp_c + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_dyml()");
            else
                return JavaScript("swal_error();");
        }
        //插入低压母联的数据
        public JavaScriptResult Insert_dyml(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_low_voltage_switcher set v_a = " + Model.elvs_v_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_switcher set v_b = " + Model.elvs_v_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_switcher set v_c = " + Model.elvs_v_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_switcher set e_a = " + Model.elvs_e_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_switcher set e_b = " + Model.elvs_e_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_switcher set e_c = " + Model.elvs_e_c + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_dy1x();");
            else
                return JavaScript("swal_error();");
        }
        //插入低压1线的数据
        public JavaScriptResult Insert_dy1x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_low_voltage_distributor_1 set v_a = " + Model.elvd1_v_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set v_b = " + Model.elvd1_v_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set v_c = " + Model.elvd1_v_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set e_a = " + Model.elvd1_e_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set e_b = " + Model.elvd1_e_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set e_c = " + Model.elvd1_e_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_1 set pf = " + Model.elvd1_pf + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_dy2x();");
            else
                return JavaScript("swal_error();");
        }
        //插入低压2线的数据
        public JavaScriptResult Insert_dy2x(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_low_voltage_distributor_2 set v_a = " + Model.elvd2_v_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set v_b = " + Model.elvd2_v_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set v_c = " + Model.elvd2_v_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set e_a = " + Model.elvd2_e_a + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set e_b = " + Model.elvd2_e_b + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set e_c = " + Model.elvd2_e_c + "where electric_id =" + Request.Form["pre_id"] + ";" +
                            "update dm_elec_low_voltage_distributor_2 set pf = " + Model.elvd2_pf + "where electric_id =" + Request.Form["pre_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_qtjc();");
            else
                return JavaScript("swal_error();");
        }
        //插入其他检查的数据
        public JavaScriptResult Insert_qtjc(ElectricReading Model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_elec_inspection_info set direct_current_state = " + Model.direct_current_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set control_room_state = " + Model.control_room_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set distributor_door_state = " + Model.distributor_door_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set electric_room_state = " + Model.electric_room_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set light_state = " + Model.light_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set wash_state = " + Model.wash_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set fire_epuip_state = " + Model.fire_epuip_state + " where electric_id =" + Request.Form["pre_id"] + ";" +
            "update dm_elec_inspection_info set safe_appliance_state = " + Model.safe_appliance_state + " where electric_id =" + Request.Form["pre_id"] + ";" + 
            "update dm_elec_inspection_info set mark = '" + Model.mark + "' where electric_id =" + Request.Form["pre_id"] + ";";

            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();");
            else
                return JavaScript("swal_error();");
        }

        //进入修改电量抄表的页面
        [HttpPost]
        public ActionResult ModifyElectricReading()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "select dm_electric_inspection.remark, " +
        "direct_current_state,control_room_state,distributor_door_state,electric_room_state,light_state,wash_state,fire_epuip_state,safe_appliance_state,mark, " +
        "dm_elec_low_voltage_distributor_1.v_a,dm_elec_low_voltage_distributor_1.v_b,dm_elec_low_voltage_distributor_1.v_c,dm_elec_low_voltage_distributor_1.e_a,dm_elec_low_voltage_distributor_1.e_b,dm_elec_low_voltage_distributor_1.e_c,dm_elec_low_voltage_distributor_1.pf," +
        "dm_elec_low_voltage_distributor_2.v_a,dm_elec_low_voltage_distributor_2.v_b,dm_elec_low_voltage_distributor_2.v_c,dm_elec_low_voltage_distributor_2.e_a,dm_elec_low_voltage_distributor_2.e_b,dm_elec_low_voltage_distributor_2.e_c,dm_elec_low_voltage_distributor_2.pf," +
        "dm_elec_low_voltage_switcher.v_a,dm_elec_low_voltage_switcher.v_b,dm_elec_low_voltage_switcher.v_c,dm_elec_low_voltage_switcher.e_a,dm_elec_low_voltage_switcher.e_b,dm_elec_low_voltage_switcher.e_c," +
        "dm_elec_high_voltage_distributor_1.v_a,dm_elec_high_voltage_distributor_1.v_b,dm_elec_high_voltage_distributor_1.v_c,dm_elec_high_voltage_distributor_1.e_a,dm_elec_high_voltage_distributor_1.e_b,dm_elec_high_voltage_distributor_1.e_c,dm_elec_high_voltage_distributor_1.transformer_temp_a,dm_elec_high_voltage_distributor_1.transformer_temp_b,dm_elec_high_voltage_distributor_1.transformer_temp_c," +
        "dm_elec_high_voltage_distributor_2.v_a,dm_elec_high_voltage_distributor_2.v_b,dm_elec_high_voltage_distributor_2.v_c,dm_elec_high_voltage_distributor_2.e_a,dm_elec_high_voltage_distributor_2.e_b,dm_elec_high_voltage_distributor_2.e_c,dm_elec_high_voltage_distributor_2.transformer_temp_a,dm_elec_high_voltage_distributor_2.transformer_temp_b,dm_elec_high_voltage_distributor_2.transformer_temp_c," +
        "dm_elec_meter_reading_1.postive_active_all,dm_elec_meter_reading_1.postive_active_sharp,dm_elec_meter_reading_1.postive_active_peak,dm_elec_meter_reading_1.postive_active_shoulder,dm_elec_meter_reading_1.postive_active_offpeak,dm_elec_meter_reading_1.postive_reactive_all,dm_elec_meter_reading_1.postive_reactive_sharp,dm_elec_meter_reading_1.postive_reactive_peak,dm_elec_meter_reading_1.postive_reactive_shoulder,dm_elec_meter_reading_1.postive_reactive_offpeak,dm_elec_meter_reading_1.pf_all,dm_elec_meter_reading_1.pf_a,dm_elec_meter_reading_1.pf_b,dm_elec_meter_reading_1.pf_c," +
        "dm_elec_meter_reading_2.postive_active_all,dm_elec_meter_reading_2.postive_active_sharp,dm_elec_meter_reading_2.postive_active_peak,dm_elec_meter_reading_2.postive_active_shoulder,dm_elec_meter_reading_2.postive_active_offpeak,dm_elec_meter_reading_2.postive_reactive_all,dm_elec_meter_reading_2.postive_reactive_sharp,dm_elec_meter_reading_2.postive_reactive_peak,dm_elec_meter_reading_2.postive_reactive_shoulder,dm_elec_meter_reading_2.postive_reactive_offpeak,dm_elec_meter_reading_2.pf_all,dm_elec_meter_reading_2.pf_a,dm_elec_meter_reading_2.pf_b,dm_elec_meter_reading_2.pf_c" +
        " ,dm_electric_inspection.id from dm_electric_inspection,dm_elec_inspection_info,dm_elec_low_voltage_distributor_1,dm_elec_low_voltage_distributor_2,dm_elec_low_voltage_switcher,dm_elec_high_voltage_distributor_1,dm_elec_high_voltage_distributor_2,dm_elec_meter_reading_1,dm_elec_meter_reading_2 " +
        "where dm_electric_inspection.id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_inspection_info.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_distributor_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_distributor_2.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_switcher.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_high_voltage_distributor_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_high_voltage_distributor_2.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_meter_reading_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_meter_reading_2.electric_id =" + Convert.ToInt32(Request.Form["erl_id"]) + ";";
            ElectricReading Model = new ElectricReading();
            SqlDataAdapter da = new SqlDataAdapter(select, con);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows[0][0] != System.DBNull.Value) Model.remark = ds.Tables[0].Rows[0][0].ToString(); else Model.remark = null;
            if (ds.Tables[0].Rows[0][1] != System.DBNull.Value) Model.direct_current_state = Convert.ToInt32(ds.Tables[0].Rows[0][1]); else Model.direct_current_state = null;
           if (ds.Tables[0].Rows[0][2] != System.DBNull.Value) Model.control_room_state = Convert.ToInt32(ds.Tables[0].Rows[0][2]); else Model.control_room_state = null;
            if (ds.Tables[0].Rows[0][3] != System.DBNull.Value) Model.distributor_door_state = Convert.ToInt32(ds.Tables[0].Rows[0][3]); else Model.distributor_door_state = null;
            if (ds.Tables[0].Rows[0][4] != System.DBNull.Value) Model.electric_room_state = Convert.ToInt32(ds.Tables[0].Rows[0][4]); else Model.electric_room_state = null;
            if (ds.Tables[0].Rows[0][5] != System.DBNull.Value) Model.light_state = Convert.ToInt32(ds.Tables[0].Rows[0][5]); else Model.light_state = null;
            if (ds.Tables[0].Rows[0][6] != System.DBNull.Value) Model.wash_state = Convert.ToInt32(ds.Tables[0].Rows[0][6]); else Model.wash_state = null;
            if (ds.Tables[0].Rows[0][7] != System.DBNull.Value) Model.fire_epuip_state = Convert.ToInt32(ds.Tables[0].Rows[0][7]); else Model.fire_epuip_state = null;
            if (ds.Tables[0].Rows[0][8] != System.DBNull.Value) Model.safe_appliance_state = Convert.ToInt32(ds.Tables[0].Rows[0][8]); else Model.safe_appliance_state = null;
            if (ds.Tables[0].Rows[0][9] != System.DBNull.Value) Model.mark = ds.Tables[0].Rows[0][9].ToString(); else Model.mark = null;
            if (ds.Tables[0].Rows[0][10] != System.DBNull.Value) Model.elvd1_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][10]); else Model.elvd1_v_a = null;
            if (ds.Tables[0].Rows[0][11] != System.DBNull.Value) Model.elvd1_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][11]); else Model.elvd1_v_b = null;
            if (ds.Tables[0].Rows[0][12] != System.DBNull.Value) Model.elvd1_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][12]); else Model.elvd1_v_c = null;
            if (ds.Tables[0].Rows[0][13] != System.DBNull.Value) Model.elvd1_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][13]); else Model.elvd1_e_a = null;
            if (ds.Tables[0].Rows[0][14] != System.DBNull.Value) Model.elvd1_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][14]); else Model.elvd1_e_b = null;
            if (ds.Tables[0].Rows[0][15] != System.DBNull.Value) Model.elvd1_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][15]); else Model.elvd1_e_c = null;
            if (ds.Tables[0].Rows[0][16] != System.DBNull.Value) Model.elvd1_pf = Convert.ToDouble(ds.Tables[0].Rows[0][16]); else Model.elvd1_pf = null;
            if (ds.Tables[0].Rows[0][17] != System.DBNull.Value) Model.elvd2_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][17]); else Model.elvd2_v_a = null;
            if (ds.Tables[0].Rows[0][18] != System.DBNull.Value) Model.elvd2_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][18]); else Model.elvd2_v_b = null;
            if (ds.Tables[0].Rows[0][19] != System.DBNull.Value) Model.elvd2_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][19]); else Model.elvd2_v_c = null;
            if (ds.Tables[0].Rows[0][20] != System.DBNull.Value) Model.elvd2_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][20]); else Model.elvd2_e_a = null;
            if (ds.Tables[0].Rows[0][21] != System.DBNull.Value) Model.elvd2_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][21]); else Model.elvd2_e_b = null;
            if (ds.Tables[0].Rows[0][22] != System.DBNull.Value) Model.elvd2_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][22]); else Model.elvd2_e_c = null;
            if (ds.Tables[0].Rows[0][23] != System.DBNull.Value) Model.elvd2_pf = Convert.ToDouble(ds.Tables[0].Rows[0][23]); else Model.elvd2_pf = null;
            if (ds.Tables[0].Rows[0][24] != System.DBNull.Value) Model.elvs_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][24]); else Model.elvs_v_a = null;
            if (ds.Tables[0].Rows[0][25] != System.DBNull.Value) Model.elvs_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][25]); else Model.elvs_v_b = null;
            if (ds.Tables[0].Rows[0][26] != System.DBNull.Value) Model.elvs_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][26]); else Model.elvs_v_c = null;
            if (ds.Tables[0].Rows[0][27] != System.DBNull.Value) Model.elvs_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][27]); else Model.elvs_e_a = null;
            if (ds.Tables[0].Rows[0][28] != System.DBNull.Value) Model.elvs_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][28]); else Model.elvs_e_b = null;
            if (ds.Tables[0].Rows[0][29] != System.DBNull.Value) Model.elvs_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][29]); else Model.elvs_e_c = null;
            if (ds.Tables[0].Rows[0][30] != System.DBNull.Value) Model.ehvd1_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][30]); else Model.ehvd1_v_a = null;
            if (ds.Tables[0].Rows[0][31] != System.DBNull.Value) Model.ehvd1_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][31]); else Model.ehvd1_v_b = null;
            if (ds.Tables[0].Rows[0][32] != System.DBNull.Value) Model.ehvd1_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][32]); else Model.ehvd1_v_c = null;
            if (ds.Tables[0].Rows[0][33] != System.DBNull.Value) Model.ehvd1_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][33]); else Model.ehvd1_e_a = null;
            if (ds.Tables[0].Rows[0][34] != System.DBNull.Value) Model.ehvd1_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][34]); else Model.ehvd1_e_b = null;
            if (ds.Tables[0].Rows[0][35] != System.DBNull.Value) Model.ehvd1_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][35]); else Model.ehvd1_e_c = null;
            if (ds.Tables[0].Rows[0][36] != System.DBNull.Value) Model.ehvd1_transformer_temp_a = Convert.ToDouble(ds.Tables[0].Rows[0][36]); else Model.ehvd1_transformer_temp_a = null;
            if (ds.Tables[0].Rows[0][37] != System.DBNull.Value) Model.ehvd1_transformer_temp_b = Convert.ToDouble(ds.Tables[0].Rows[0][37]); else Model.ehvd1_transformer_temp_b = null;
            if (ds.Tables[0].Rows[0][38] != System.DBNull.Value) Model.ehvd1_transformer_temp_c = Convert.ToDouble(ds.Tables[0].Rows[0][38]); else Model.ehvd1_transformer_temp_c = null;
            if (ds.Tables[0].Rows[0][39] != System.DBNull.Value) Model.ehvd2_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][39]); else Model.ehvd2_v_a = null;
            if (ds.Tables[0].Rows[0][40] != System.DBNull.Value) Model.ehvd2_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][40]); else Model.ehvd2_v_b = null;
            if (ds.Tables[0].Rows[0][41] != System.DBNull.Value) Model.ehvd2_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][41]); else Model.ehvd2_v_c = null;
            if (ds.Tables[0].Rows[0][42] != System.DBNull.Value) Model.ehvd2_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][42]); else Model.ehvd2_e_a = null;
            if (ds.Tables[0].Rows[0][43] != System.DBNull.Value) Model.ehvd2_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][43]); else Model.ehvd2_e_b = null;
            if (ds.Tables[0].Rows[0][44] != System.DBNull.Value) Model.ehvd2_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][44]); else Model.ehvd2_e_c = null;
            if (ds.Tables[0].Rows[0][45] != System.DBNull.Value) Model.ehvd2_transformer_temp_a = Convert.ToDouble(ds.Tables[0].Rows[0][45]); else Model.ehvd2_transformer_temp_a = null;
            if (ds.Tables[0].Rows[0][46] != System.DBNull.Value) Model.ehvd2_transformer_temp_b = Convert.ToDouble(ds.Tables[0].Rows[0][46]); else Model.ehvd2_transformer_temp_b = null;
            if (ds.Tables[0].Rows[0][47] != System.DBNull.Value) Model.ehvd2_transformer_temp_c = Convert.ToDouble(ds.Tables[0].Rows[0][47]); else Model.ehvd2_transformer_temp_c = null;
            if (ds.Tables[0].Rows[0][48] != System.DBNull.Value) Model.emr1_postive_active_all = Convert.ToDouble(ds.Tables[0].Rows[0][48]); else Model.emr1_postive_active_all = null;
            if (ds.Tables[0].Rows[0][49] != System.DBNull.Value) Model.emr1_postive_active_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][49]); else Model.emr1_postive_active_sharp = null;
            if (ds.Tables[0].Rows[0][50] != System.DBNull.Value) Model.emr1_postive_active_peak = Convert.ToDouble(ds.Tables[0].Rows[0][50]); else Model.emr1_postive_active_peak = null;
            if (ds.Tables[0].Rows[0][51] != System.DBNull.Value) Model.emr1_postive_active_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][51]); else Model.emr1_postive_active_shoulder = null;
            if (ds.Tables[0].Rows[0][52] != System.DBNull.Value) Model.emr1_postive_active_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][52]); else Model.emr1_postive_active_offpeak = null;
            if (ds.Tables[0].Rows[0][53] != System.DBNull.Value) Model.emr1_postive_reactive_all = Convert.ToDouble(ds.Tables[0].Rows[0][53]); else Model.emr1_postive_reactive_all = null;
            if (ds.Tables[0].Rows[0][54] != System.DBNull.Value) Model.emr1_postive_reactive_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][54]); else Model.emr1_postive_reactive_sharp = null;
            if (ds.Tables[0].Rows[0][55] != System.DBNull.Value) Model.emr1_postive_reactive_peak = Convert.ToDouble(ds.Tables[0].Rows[0][55]); else Model.emr1_postive_reactive_peak = null;
            if (ds.Tables[0].Rows[0][56] != System.DBNull.Value) Model.emr1_postive_reactive_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][56]); else Model.emr1_postive_reactive_shoulder = null;
            if (ds.Tables[0].Rows[0][57] != System.DBNull.Value) Model.emr1_postive_reactive_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][57]); else Model.emr1_postive_reactive_offpeak = null;
            if (ds.Tables[0].Rows[0][58] != System.DBNull.Value) Model.emr1_pf_all = Convert.ToDouble(ds.Tables[0].Rows[0][58]); else Model.emr1_pf_all = null;
            if (ds.Tables[0].Rows[0][59] != System.DBNull.Value) Model.emr1_pf_a = Convert.ToDouble(ds.Tables[0].Rows[0][59]); else Model.emr1_pf_a = null;
            if (ds.Tables[0].Rows[0][60] != System.DBNull.Value) Model.emr1_pf_b = Convert.ToDouble(ds.Tables[0].Rows[0][60]); else Model.emr1_pf_b = null;
            if (ds.Tables[0].Rows[0][61] != System.DBNull.Value) Model.emr1_pf_c = Convert.ToDouble(ds.Tables[0].Rows[0][61]); else Model.emr1_pf_c = null;
            if (ds.Tables[0].Rows[0][62] != System.DBNull.Value) Model.emr2_postive_active_all = Convert.ToDouble(ds.Tables[0].Rows[0][62]); else Model.emr2_postive_active_all = null;
            if (ds.Tables[0].Rows[0][63] != System.DBNull.Value) Model.emr2_postive_active_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][63]); else Model.emr2_postive_active_sharp = null;
            if (ds.Tables[0].Rows[0][64] != System.DBNull.Value) Model.emr2_postive_active_peak = Convert.ToDouble(ds.Tables[0].Rows[0][64]); else Model.emr2_postive_active_peak = null;
            if (ds.Tables[0].Rows[0][65] != System.DBNull.Value) Model.emr2_postive_active_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][65]); else Model.emr2_postive_active_shoulder = null;
            if (ds.Tables[0].Rows[0][66] != System.DBNull.Value) Model.emr2_postive_active_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][66]); else Model.emr2_postive_active_offpeak = null;
            if (ds.Tables[0].Rows[0][67] != System.DBNull.Value) Model.emr2_postive_reactive_all = Convert.ToDouble(ds.Tables[0].Rows[0][67]); else Model.emr2_postive_reactive_all = null;
            if (ds.Tables[0].Rows[0][68] != System.DBNull.Value) Model.emr2_postive_reactive_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][68]); else Model.emr2_postive_reactive_sharp = null;
            if (ds.Tables[0].Rows[0][69] != System.DBNull.Value) Model.emr2_postive_reactive_peak = Convert.ToDouble(ds.Tables[0].Rows[0][69]); else Model.emr2_postive_reactive_peak = null;
            if (ds.Tables[0].Rows[0][70] != System.DBNull.Value) Model.emr2_postive_reactive_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][70]); else Model.emr2_postive_reactive_shoulder = null;
            if (ds.Tables[0].Rows[0][71] != System.DBNull.Value) Model.emr2_postive_reactive_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][71]); else Model.emr2_postive_reactive_offpeak = null;
            if (ds.Tables[0].Rows[0][72] != System.DBNull.Value) Model.emr2_pf_all = Convert.ToDouble(ds.Tables[0].Rows[0][72]); else Model.emr2_pf_all = null;
            if (ds.Tables[0].Rows[0][73] != System.DBNull.Value) Model.emr2_pf_a = Convert.ToDouble(ds.Tables[0].Rows[0][73]); else Model.emr2_pf_a = null;
            if (ds.Tables[0].Rows[0][74] != System.DBNull.Value) Model.emr2_pf_b = Convert.ToDouble(ds.Tables[0].Rows[0][74]); else Model.emr2_pf_b = null;
            if (ds.Tables[0].Rows[0][75] != System.DBNull.Value) Model.emr2_pf_c = Convert.ToDouble(ds.Tables[0].Rows[0][75]); else Model.emr2_pf_c = null;
            if (ds.Tables[0].Rows[0][76] != System.DBNull.Value) Model.total_id = Convert.ToInt32(ds.Tables[0].Rows[0][76]); else Model.total_id = null;
            con.Close();
            return View(Model);
        }

        public JavaScriptResult ModifyBeginElecReading()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "update dm_electric_inspection set remark = '" + Request.Form["aer_remark"] + "' where id = " + Request.Form["total_id"] + ";" +
                            "update dm_electric_inspection set user_id = '" + Session["user_id"] + "' where id = " + Request.Form["total_id"] + ";" +
                            "update dm_electric_inspection set user_name = '" + Session["real_name"] + "' where id = " + Request.Form["total_id"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            var check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
                return JavaScript("swal_success();jump_to_db1x()");
            else
                return JavaScript("swal_error();");
        }

        //删除电力数据
        public void DeleteElecReading()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "delete dm_elec_inspection_info where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_low_voltage_distributor_1 where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_low_voltage_distributor_2 where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_low_voltage_switcher where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_high_voltage_distributor_1 where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_high_voltage_distributor_2 where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_meter_reading_1 where electric_id = " + Request.Form["erl_del"] + ";" +
                            "delete dm_elec_meter_reading_2 where electric_id = " + Request.Form["erl_del"] + ";" + 
                            "delete dm_electric_inspection where id = " + Request.Form["erl_del"] + ";";
            SqlCommand cmd = new SqlCommand(select, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check > 0)
            {
                Response.Write("<script>alert('以成功删除巡查记录！！');window.location.href='ElecReadingList';</script>");
            }
            else
                Response.Write("<script>alert('删除失败！,请手动联系管理员~');</script>");
        }
        //导出电力巡查Excel文本
        public void ExcelElecReading()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string select = "select dm_electric_inspection.remark, " +
        "direct_current_state,control_room_state,distributor_door_state,electric_room_state,light_state,wash_state,fire_epuip_state,safe_appliance_state,mark, " +
        "dm_elec_low_voltage_distributor_1.v_a,dm_elec_low_voltage_distributor_1.v_b,dm_elec_low_voltage_distributor_1.v_c,dm_elec_low_voltage_distributor_1.e_a,dm_elec_low_voltage_distributor_1.e_b,dm_elec_low_voltage_distributor_1.e_c,dm_elec_low_voltage_distributor_1.pf," +
        "dm_elec_low_voltage_distributor_2.v_a,dm_elec_low_voltage_distributor_2.v_b,dm_elec_low_voltage_distributor_2.v_c,dm_elec_low_voltage_distributor_2.e_a,dm_elec_low_voltage_distributor_2.e_b,dm_elec_low_voltage_distributor_2.e_c,dm_elec_low_voltage_distributor_2.pf," +
        "dm_elec_low_voltage_switcher.v_a,dm_elec_low_voltage_switcher.v_b,dm_elec_low_voltage_switcher.v_c,dm_elec_low_voltage_switcher.e_a,dm_elec_low_voltage_switcher.e_b,dm_elec_low_voltage_switcher.e_c," +
        "dm_elec_high_voltage_distributor_1.v_a,dm_elec_high_voltage_distributor_1.v_b,dm_elec_high_voltage_distributor_1.v_c,dm_elec_high_voltage_distributor_1.e_a,dm_elec_high_voltage_distributor_1.e_b,dm_elec_high_voltage_distributor_1.e_c,dm_elec_high_voltage_distributor_1.transformer_temp_a,dm_elec_high_voltage_distributor_1.transformer_temp_b,dm_elec_high_voltage_distributor_1.transformer_temp_c," +
        "dm_elec_high_voltage_distributor_2.v_a,dm_elec_high_voltage_distributor_2.v_b,dm_elec_high_voltage_distributor_2.v_c,dm_elec_high_voltage_distributor_2.e_a,dm_elec_high_voltage_distributor_2.e_b,dm_elec_high_voltage_distributor_2.e_c,dm_elec_high_voltage_distributor_2.transformer_temp_a,dm_elec_high_voltage_distributor_2.transformer_temp_b,dm_elec_high_voltage_distributor_2.transformer_temp_c," +
        "dm_elec_meter_reading_1.postive_active_all,dm_elec_meter_reading_1.postive_active_sharp,dm_elec_meter_reading_1.postive_active_peak,dm_elec_meter_reading_1.postive_active_shoulder,dm_elec_meter_reading_1.postive_active_offpeak,dm_elec_meter_reading_1.postive_reactive_all,dm_elec_meter_reading_1.postive_reactive_sharp,dm_elec_meter_reading_1.postive_reactive_peak,dm_elec_meter_reading_1.postive_reactive_shoulder,dm_elec_meter_reading_1.postive_reactive_offpeak,dm_elec_meter_reading_1.pf_all,dm_elec_meter_reading_1.pf_a,dm_elec_meter_reading_1.pf_b,dm_elec_meter_reading_1.pf_c," +
        "dm_elec_meter_reading_2.postive_active_all,dm_elec_meter_reading_2.postive_active_sharp,dm_elec_meter_reading_2.postive_active_peak,dm_elec_meter_reading_2.postive_active_shoulder,dm_elec_meter_reading_2.postive_active_offpeak,dm_elec_meter_reading_2.postive_reactive_all,dm_elec_meter_reading_2.postive_reactive_sharp,dm_elec_meter_reading_2.postive_reactive_peak,dm_elec_meter_reading_2.postive_reactive_shoulder,dm_elec_meter_reading_2.postive_reactive_offpeak,dm_elec_meter_reading_2.pf_all,dm_elec_meter_reading_2.pf_a,dm_elec_meter_reading_2.pf_b,dm_elec_meter_reading_2.pf_c" +
        " ,dm_electric_inspection.id,dm_electric_inspection.add_time,dm_electric_inspection.user_name from dm_electric_inspection,dm_elec_inspection_info,dm_elec_low_voltage_distributor_1,dm_elec_low_voltage_distributor_2,dm_elec_low_voltage_switcher,dm_elec_high_voltage_distributor_1,dm_elec_high_voltage_distributor_2,dm_elec_meter_reading_1,dm_elec_meter_reading_2 " +
        "where dm_electric_inspection.id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_inspection_info.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_distributor_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_distributor_2.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_low_voltage_switcher.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_high_voltage_distributor_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_high_voltage_distributor_2.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_meter_reading_1.electric_id = " + Convert.ToInt32(Request.Form["erl_id"]) + " and " +
              "dm_elec_meter_reading_2.electric_id =" + Convert.ToInt32(Request.Form["erl_id"]) + ";";
            ElectricReading Model = new ElectricReading();
            SqlDataAdapter da = new SqlDataAdapter(select, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows[0][0] != System.DBNull.Value) Model.remark = ds.Tables[0].Rows[0][0].ToString(); else Model.remark = null;
            if (ds.Tables[0].Rows[0][1] != System.DBNull.Value) Model.direct_current_state = Convert.ToInt32(ds.Tables[0].Rows[0][1]); else Model.direct_current_state = null;
            if (ds.Tables[0].Rows[0][2] != System.DBNull.Value) Model.control_room_state = Convert.ToInt32(ds.Tables[0].Rows[0][2]); else Model.control_room_state = null;
            if (ds.Tables[0].Rows[0][3] != System.DBNull.Value) Model.distributor_door_state = Convert.ToInt32(ds.Tables[0].Rows[0][3]); else Model.distributor_door_state = null;
            if (ds.Tables[0].Rows[0][4] != System.DBNull.Value) Model.electric_room_state = Convert.ToInt32(ds.Tables[0].Rows[0][4]); else Model.electric_room_state = null;
            if (ds.Tables[0].Rows[0][5] != System.DBNull.Value) Model.light_state = Convert.ToInt32(ds.Tables[0].Rows[0][5]); else Model.light_state = null;
            if (ds.Tables[0].Rows[0][6] != System.DBNull.Value) Model.wash_state = Convert.ToInt32(ds.Tables[0].Rows[0][6]); else Model.wash_state = null;
            if (ds.Tables[0].Rows[0][7] != System.DBNull.Value) Model.fire_epuip_state = Convert.ToInt32(ds.Tables[0].Rows[0][7]); else Model.fire_epuip_state = null;
            if (ds.Tables[0].Rows[0][8] != System.DBNull.Value) Model.safe_appliance_state = Convert.ToInt32(ds.Tables[0].Rows[0][8]); else Model.safe_appliance_state = null;
            if (ds.Tables[0].Rows[0][9] != System.DBNull.Value) Model.mark = ds.Tables[0].Rows[0][9].ToString(); else Model.mark = null;
            if (ds.Tables[0].Rows[0][10] != System.DBNull.Value) Model.elvd1_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][10]); else Model.elvd1_v_a = null;
            if (ds.Tables[0].Rows[0][11] != System.DBNull.Value) Model.elvd1_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][11]); else Model.elvd1_v_b = null;
            if (ds.Tables[0].Rows[0][12] != System.DBNull.Value) Model.elvd1_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][12]); else Model.elvd1_v_c = null;
            if (ds.Tables[0].Rows[0][13] != System.DBNull.Value) Model.elvd1_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][13]); else Model.elvd1_e_a = null;
            if (ds.Tables[0].Rows[0][14] != System.DBNull.Value) Model.elvd1_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][14]); else Model.elvd1_e_b = null;
            if (ds.Tables[0].Rows[0][15] != System.DBNull.Value) Model.elvd1_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][15]); else Model.elvd1_e_c = null;
            if (ds.Tables[0].Rows[0][16] != System.DBNull.Value) Model.elvd1_pf = Convert.ToDouble(ds.Tables[0].Rows[0][16]); else Model.elvd1_pf = null;
            if (ds.Tables[0].Rows[0][17] != System.DBNull.Value) Model.elvd2_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][17]); else Model.elvd2_v_a = null;
            if (ds.Tables[0].Rows[0][18] != System.DBNull.Value) Model.elvd2_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][18]); else Model.elvd2_v_b = null;
            if (ds.Tables[0].Rows[0][19] != System.DBNull.Value) Model.elvd2_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][19]); else Model.elvd2_v_c = null;
            if (ds.Tables[0].Rows[0][20] != System.DBNull.Value) Model.elvd2_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][20]); else Model.elvd2_e_a = null;
            if (ds.Tables[0].Rows[0][21] != System.DBNull.Value) Model.elvd2_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][21]); else Model.elvd2_e_b = null;
            if (ds.Tables[0].Rows[0][22] != System.DBNull.Value) Model.elvd2_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][22]); else Model.elvd2_e_c = null;
            if (ds.Tables[0].Rows[0][23] != System.DBNull.Value) Model.elvd2_pf = Convert.ToDouble(ds.Tables[0].Rows[0][23]); else Model.elvd2_pf = null;
            if (ds.Tables[0].Rows[0][24] != System.DBNull.Value) Model.elvs_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][24]); else Model.elvs_v_a = null;
            if (ds.Tables[0].Rows[0][25] != System.DBNull.Value) Model.elvs_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][25]); else Model.elvs_v_b = null;
            if (ds.Tables[0].Rows[0][26] != System.DBNull.Value) Model.elvs_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][26]); else Model.elvs_v_c = null;
            if (ds.Tables[0].Rows[0][27] != System.DBNull.Value) Model.elvs_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][27]); else Model.elvs_e_a = null;
            if (ds.Tables[0].Rows[0][28] != System.DBNull.Value) Model.elvs_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][28]); else Model.elvs_e_b = null;
            if (ds.Tables[0].Rows[0][29] != System.DBNull.Value) Model.elvs_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][29]); else Model.elvs_e_c = null;
            if (ds.Tables[0].Rows[0][30] != System.DBNull.Value) Model.ehvd1_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][30]); else Model.ehvd1_v_a = null;
            if (ds.Tables[0].Rows[0][31] != System.DBNull.Value) Model.ehvd1_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][31]); else Model.ehvd1_v_b = null;
            if (ds.Tables[0].Rows[0][32] != System.DBNull.Value) Model.ehvd1_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][32]); else Model.ehvd1_v_c = null;
            if (ds.Tables[0].Rows[0][33] != System.DBNull.Value) Model.ehvd1_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][33]); else Model.ehvd1_e_a = null;
            if (ds.Tables[0].Rows[0][34] != System.DBNull.Value) Model.ehvd1_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][34]); else Model.ehvd1_e_b = null;
            if (ds.Tables[0].Rows[0][35] != System.DBNull.Value) Model.ehvd1_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][35]); else Model.ehvd1_e_c = null;
            if (ds.Tables[0].Rows[0][36] != System.DBNull.Value) Model.ehvd1_transformer_temp_a = Convert.ToDouble(ds.Tables[0].Rows[0][36]); else Model.ehvd1_transformer_temp_a = null;
            if (ds.Tables[0].Rows[0][37] != System.DBNull.Value) Model.ehvd1_transformer_temp_b = Convert.ToDouble(ds.Tables[0].Rows[0][37]); else Model.ehvd1_transformer_temp_b = null;
            if (ds.Tables[0].Rows[0][38] != System.DBNull.Value) Model.ehvd1_transformer_temp_c = Convert.ToDouble(ds.Tables[0].Rows[0][38]); else Model.ehvd1_transformer_temp_c = null;
            if (ds.Tables[0].Rows[0][39] != System.DBNull.Value) Model.ehvd2_v_a = Convert.ToDouble(ds.Tables[0].Rows[0][39]); else Model.ehvd2_v_a = null;
            if (ds.Tables[0].Rows[0][40] != System.DBNull.Value) Model.ehvd2_v_b = Convert.ToDouble(ds.Tables[0].Rows[0][40]); else Model.ehvd2_v_b = null;
            if (ds.Tables[0].Rows[0][41] != System.DBNull.Value) Model.ehvd2_v_c = Convert.ToDouble(ds.Tables[0].Rows[0][41]); else Model.ehvd2_v_c = null;
            if (ds.Tables[0].Rows[0][42] != System.DBNull.Value) Model.ehvd2_e_a = Convert.ToDouble(ds.Tables[0].Rows[0][42]); else Model.ehvd2_e_a = null;
            if (ds.Tables[0].Rows[0][43] != System.DBNull.Value) Model.ehvd2_e_b = Convert.ToDouble(ds.Tables[0].Rows[0][43]); else Model.ehvd2_e_b = null;
            if (ds.Tables[0].Rows[0][44] != System.DBNull.Value) Model.ehvd2_e_c = Convert.ToDouble(ds.Tables[0].Rows[0][44]); else Model.ehvd2_e_c = null;
            if (ds.Tables[0].Rows[0][45] != System.DBNull.Value) Model.ehvd2_transformer_temp_a = Convert.ToDouble(ds.Tables[0].Rows[0][45]); else Model.ehvd2_transformer_temp_a = null;
            if (ds.Tables[0].Rows[0][46] != System.DBNull.Value) Model.ehvd2_transformer_temp_b = Convert.ToDouble(ds.Tables[0].Rows[0][46]); else Model.ehvd2_transformer_temp_b = null;
            if (ds.Tables[0].Rows[0][47] != System.DBNull.Value) Model.ehvd2_transformer_temp_c = Convert.ToDouble(ds.Tables[0].Rows[0][47]); else Model.ehvd2_transformer_temp_c = null;
            if (ds.Tables[0].Rows[0][48] != System.DBNull.Value) Model.emr1_postive_active_all = Convert.ToDouble(ds.Tables[0].Rows[0][48]); else Model.emr1_postive_active_all = null;
            if (ds.Tables[0].Rows[0][49] != System.DBNull.Value) Model.emr1_postive_active_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][49]); else Model.emr1_postive_active_sharp = null;
            if (ds.Tables[0].Rows[0][50] != System.DBNull.Value) Model.emr1_postive_active_peak = Convert.ToDouble(ds.Tables[0].Rows[0][50]); else Model.emr1_postive_active_peak = null;
            if (ds.Tables[0].Rows[0][51] != System.DBNull.Value) Model.emr1_postive_active_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][51]); else Model.emr1_postive_active_shoulder = null;
            if (ds.Tables[0].Rows[0][52] != System.DBNull.Value) Model.emr1_postive_active_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][52]); else Model.emr1_postive_active_offpeak = null;
            if (ds.Tables[0].Rows[0][53] != System.DBNull.Value) Model.emr1_postive_reactive_all = Convert.ToDouble(ds.Tables[0].Rows[0][53]); else Model.emr1_postive_reactive_all = null;
            if (ds.Tables[0].Rows[0][54] != System.DBNull.Value) Model.emr1_postive_reactive_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][54]); else Model.emr1_postive_reactive_sharp = null;
            if (ds.Tables[0].Rows[0][55] != System.DBNull.Value) Model.emr1_postive_reactive_peak = Convert.ToDouble(ds.Tables[0].Rows[0][55]); else Model.emr1_postive_reactive_peak = null;
            if (ds.Tables[0].Rows[0][56] != System.DBNull.Value) Model.emr1_postive_reactive_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][56]); else Model.emr1_postive_reactive_shoulder = null;
            if (ds.Tables[0].Rows[0][57] != System.DBNull.Value) Model.emr1_postive_reactive_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][57]); else Model.emr1_postive_reactive_offpeak = null;
            if (ds.Tables[0].Rows[0][58] != System.DBNull.Value) Model.emr1_pf_all = Convert.ToDouble(ds.Tables[0].Rows[0][58]); else Model.emr1_pf_all = null;
            if (ds.Tables[0].Rows[0][59] != System.DBNull.Value) Model.emr1_pf_a = Convert.ToDouble(ds.Tables[0].Rows[0][59]); else Model.emr1_pf_a = null;
            if (ds.Tables[0].Rows[0][60] != System.DBNull.Value) Model.emr1_pf_b = Convert.ToDouble(ds.Tables[0].Rows[0][60]); else Model.emr1_pf_b = null;
            if (ds.Tables[0].Rows[0][61] != System.DBNull.Value) Model.emr1_pf_c = Convert.ToDouble(ds.Tables[0].Rows[0][61]); else Model.emr1_pf_c = null;
            if (ds.Tables[0].Rows[0][62] != System.DBNull.Value) Model.emr2_postive_active_all = Convert.ToDouble(ds.Tables[0].Rows[0][62]); else Model.emr2_postive_active_all = null;
            if (ds.Tables[0].Rows[0][63] != System.DBNull.Value) Model.emr2_postive_active_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][63]); else Model.emr2_postive_active_sharp = null;
            if (ds.Tables[0].Rows[0][64] != System.DBNull.Value) Model.emr2_postive_active_peak = Convert.ToDouble(ds.Tables[0].Rows[0][64]); else Model.emr2_postive_active_peak = null;
            if (ds.Tables[0].Rows[0][65] != System.DBNull.Value) Model.emr2_postive_active_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][65]); else Model.emr2_postive_active_shoulder = null;
            if (ds.Tables[0].Rows[0][66] != System.DBNull.Value) Model.emr2_postive_active_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][66]); else Model.emr2_postive_active_offpeak = null;
            if (ds.Tables[0].Rows[0][67] != System.DBNull.Value) Model.emr2_postive_reactive_all = Convert.ToDouble(ds.Tables[0].Rows[0][67]); else Model.emr2_postive_reactive_all = null;
            if (ds.Tables[0].Rows[0][68] != System.DBNull.Value) Model.emr2_postive_reactive_sharp = Convert.ToDouble(ds.Tables[0].Rows[0][68]); else Model.emr2_postive_reactive_sharp = null;
            if (ds.Tables[0].Rows[0][69] != System.DBNull.Value) Model.emr2_postive_reactive_peak = Convert.ToDouble(ds.Tables[0].Rows[0][69]); else Model.emr2_postive_reactive_peak = null;
            if (ds.Tables[0].Rows[0][70] != System.DBNull.Value) Model.emr2_postive_reactive_shoulder = Convert.ToDouble(ds.Tables[0].Rows[0][70]); else Model.emr2_postive_reactive_shoulder = null;
            if (ds.Tables[0].Rows[0][71] != System.DBNull.Value) Model.emr2_postive_reactive_offpeak = Convert.ToDouble(ds.Tables[0].Rows[0][71]); else Model.emr2_postive_reactive_offpeak = null;
            if (ds.Tables[0].Rows[0][72] != System.DBNull.Value) Model.emr2_pf_all = Convert.ToDouble(ds.Tables[0].Rows[0][72]); else Model.emr2_pf_all = null;
            if (ds.Tables[0].Rows[0][73] != System.DBNull.Value) Model.emr2_pf_a = Convert.ToDouble(ds.Tables[0].Rows[0][73]); else Model.emr2_pf_a = null;
            if (ds.Tables[0].Rows[0][74] != System.DBNull.Value) Model.emr2_pf_b = Convert.ToDouble(ds.Tables[0].Rows[0][74]); else Model.emr2_pf_b = null;
            if (ds.Tables[0].Rows[0][75] != System.DBNull.Value) Model.emr2_pf_c = Convert.ToDouble(ds.Tables[0].Rows[0][75]); else Model.emr2_pf_c = null;
            if (ds.Tables[0].Rows[0][76] != System.DBNull.Value) Model.total_id = Convert.ToInt32(ds.Tables[0].Rows[0][76]); else Model.total_id = null;
            Model.add_time = Convert.ToDateTime(ds.Tables[0].Rows[0][77]);
            Model.user_name = ds.Tables[0].Rows[0][78].ToString();
            con.Close();
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            //打开模板文件到文件流中
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\ElecReading.xls", FileMode.Open, FileAccess.Read))
            {
                //将文件流中模板加载到工作簿对象中
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //巡查时间及巡查人赋值
                sheet1.GetRow(1).GetCell(0).SetCellValue("日期："+ Model.add_time.ToString("D") +"    时间：" + Model.add_time.ToString("t") +"    巡查人："+Model.user_name);
                //电表1线赋值
                sheet1.GetRow(4).GetCell(3).SetCellValue(Model.emr1_postive_active_all.ToString());
                sheet1.GetRow(5).GetCell(3).SetCellValue(Model.emr1_postive_active_sharp.ToString());
                sheet1.GetRow(6).GetCell(3).SetCellValue(Model.emr1_postive_active_peak.ToString());
                sheet1.GetRow(7).GetCell(3).SetCellValue(Model.emr1_postive_active_shoulder.ToString());
                sheet1.GetRow(8).GetCell(3).SetCellValue(Model.emr1_postive_active_offpeak.ToString());
                sheet1.GetRow(9).GetCell(3).SetCellValue(Model.emr1_postive_reactive_all.ToString());
                sheet1.GetRow(10).GetCell(3).SetCellValue(Model.emr1_postive_reactive_sharp.ToString());
                sheet1.GetRow(11).GetCell(3).SetCellValue(Model.emr1_postive_reactive_peak.ToString());
                sheet1.GetRow(12).GetCell(3).SetCellValue(Model.emr1_postive_reactive_shoulder.ToString());
                sheet1.GetRow(13).GetCell(3).SetCellValue(Model.emr1_postive_reactive_offpeak.ToString());
                sheet1.GetRow(14).GetCell(3).SetCellValue(Model.emr1_pf_all.ToString());
                sheet1.GetRow(15).GetCell(3).SetCellValue(Model.emr1_pf_a.ToString());
                sheet1.GetRow(16).GetCell(3).SetCellValue(Model.emr1_pf_b.ToString());
                sheet1.GetRow(17).GetCell(3).SetCellValue(Model.emr1_pf_c.ToString());
                //电表2线赋值
                sheet1.GetRow(4).GetCell(4).SetCellValue(Model.emr2_postive_active_all.ToString());
                sheet1.GetRow(5).GetCell(4).SetCellValue(Model.emr2_postive_active_sharp.ToString());
                sheet1.GetRow(6).GetCell(4).SetCellValue(Model.emr2_postive_active_peak.ToString());
                sheet1.GetRow(7).GetCell(4).SetCellValue(Model.emr2_postive_active_shoulder.ToString());
                sheet1.GetRow(8).GetCell(4).SetCellValue(Model.emr2_postive_active_offpeak.ToString());
                sheet1.GetRow(9).GetCell(4).SetCellValue(Model.emr2_postive_reactive_all.ToString());
                sheet1.GetRow(10).GetCell(4).SetCellValue(Model.emr2_postive_reactive_sharp.ToString());
                sheet1.GetRow(11).GetCell(4).SetCellValue(Model.emr2_postive_reactive_peak.ToString());
                sheet1.GetRow(12).GetCell(4).SetCellValue(Model.emr2_postive_reactive_shoulder.ToString());
                sheet1.GetRow(13).GetCell(4).SetCellValue(Model.emr2_postive_reactive_offpeak.ToString());
                sheet1.GetRow(14).GetCell(4).SetCellValue(Model.emr2_pf_all.ToString());
                sheet1.GetRow(15).GetCell(4).SetCellValue(Model.emr2_pf_a.ToString());
                sheet1.GetRow(16).GetCell(4).SetCellValue(Model.emr2_pf_b.ToString());
                sheet1.GetRow(17).GetCell(4).SetCellValue(Model.emr2_pf_c.ToString());
                //高压1线赋值
                sheet1.GetRow(18).GetCell(3).SetCellValue(Model.ehvd1_v_a.ToString());
                sheet1.GetRow(19).GetCell(3).SetCellValue(Model.ehvd1_v_b.ToString());
                sheet1.GetRow(20).GetCell(3).SetCellValue(Model.ehvd1_v_c.ToString());
                sheet1.GetRow(21).GetCell(3).SetCellValue(Model.ehvd1_e_a.ToString());
                sheet1.GetRow(22).GetCell(3).SetCellValue(Model.ehvd1_e_b.ToString());
                sheet1.GetRow(23).GetCell(3).SetCellValue(Model.ehvd1_e_c.ToString());
                sheet1.GetRow(24).GetCell(3).SetCellValue(Model.ehvd1_transformer_temp_a.ToString());
                sheet1.GetRow(25).GetCell(3).SetCellValue(Model.ehvd1_transformer_temp_b.ToString());
                sheet1.GetRow(26).GetCell(3).SetCellValue(Model.ehvd1_transformer_temp_c.ToString());
                //高压2线赋值
                sheet1.GetRow(18).GetCell(4).SetCellValue(Model.ehvd2_v_a.ToString());
                sheet1.GetRow(19).GetCell(4).SetCellValue(Model.ehvd2_v_b.ToString());
                sheet1.GetRow(20).GetCell(4).SetCellValue(Model.ehvd2_v_c.ToString());
                sheet1.GetRow(21).GetCell(4).SetCellValue(Model.ehvd2_e_a.ToString());
                sheet1.GetRow(22).GetCell(4).SetCellValue(Model.ehvd2_e_b.ToString());
                sheet1.GetRow(23).GetCell(4).SetCellValue(Model.ehvd2_e_c.ToString());
                sheet1.GetRow(24).GetCell(4).SetCellValue(Model.ehvd2_transformer_temp_a.ToString());
                sheet1.GetRow(25).GetCell(4).SetCellValue(Model.ehvd2_transformer_temp_b.ToString());
                sheet1.GetRow(26).GetCell(4).SetCellValue(Model.ehvd2_transformer_temp_c.ToString());
                //低压母联赋值
                sheet1.GetRow(27).GetCell(3).SetCellValue(Model.elvs_v_a.ToString());
                sheet1.GetRow(27).GetCell(4).SetCellValue(Model.elvs_v_b.ToString());
                sheet1.GetRow(27).GetCell(5).SetCellValue(Model.elvs_v_c.ToString());
                sheet1.GetRow(28).GetCell(3).SetCellValue(Model.elvs_e_a.ToString());
                sheet1.GetRow(28).GetCell(4).SetCellValue(Model.elvs_e_b.ToString());
                sheet1.GetRow(28).GetCell(5).SetCellValue(Model.elvs_e_c.ToString());
                //低压1线
                sheet1.GetRow(29).GetCell(3).SetCellValue(Model.elvd1_v_a.ToString());
                sheet1.GetRow(30).GetCell(3).SetCellValue(Model.elvd1_v_b.ToString());
                sheet1.GetRow(31).GetCell(3).SetCellValue(Model.elvd1_v_c.ToString());
                sheet1.GetRow(32).GetCell(3).SetCellValue(Model.elvd1_e_a.ToString());
                sheet1.GetRow(33).GetCell(3).SetCellValue(Model.elvd1_e_b.ToString());
                sheet1.GetRow(34).GetCell(3).SetCellValue(Model.elvd1_e_c.ToString());
                sheet1.GetRow(35).GetCell(3).SetCellValue(Model.elvd1_pf.ToString());
                //低压2线
                sheet1.GetRow(29).GetCell(4).SetCellValue(Model.elvd2_v_a.ToString());
                sheet1.GetRow(30).GetCell(4).SetCellValue(Model.elvd2_v_b.ToString());
                sheet1.GetRow(31).GetCell(4).SetCellValue(Model.elvd2_v_c.ToString());
                sheet1.GetRow(32).GetCell(4).SetCellValue(Model.elvd2_e_a.ToString());
                sheet1.GetRow(33).GetCell(4).SetCellValue(Model.elvd2_e_b.ToString());
                sheet1.GetRow(34).GetCell(4).SetCellValue(Model.elvd2_e_c.ToString());
                sheet1.GetRow(35).GetCell(4).SetCellValue(Model.elvd2_pf.ToString());
                //其他检查
                if(Model.direct_current_state == 0) sheet1.GetRow(36).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(36).GetCell(2).SetCellValue("正常");
                if (Model.control_room_state == 0) sheet1.GetRow(37).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(37).GetCell(2).SetCellValue("正常");
                if (Model.distributor_door_state == 0) sheet1.GetRow(38).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(38).GetCell(2).SetCellValue("正常");
                if (Model.electric_room_state == 0) sheet1.GetRow(39).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(39).GetCell(2).SetCellValue("正常");
                if (Model.light_state == 0) sheet1.GetRow(40).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(40).GetCell(2).SetCellValue("正常");
                if (Model.wash_state == 0) sheet1.GetRow(41).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(41).GetCell(2).SetCellValue("正常");
                if (Model.fire_epuip_state == 0) sheet1.GetRow(42).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(42).GetCell(2).SetCellValue("正常");
                if (Model.safe_appliance_state == 0) sheet1.GetRow(43).GetCell(2).SetCellValue("异常"); else sheet1.GetRow(43).GetCell(2).SetCellValue("正常");
                sheet1.GetRow(44).GetCell(0).SetCellValue("备注：" + Model.mark);
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, Model.add_time.ToString("D")  );

            }
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
    }

}




