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
    }

}




