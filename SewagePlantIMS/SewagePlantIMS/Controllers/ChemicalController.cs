using SewagePlantIMS.Filter;
using SewagePlantIMS.Function;
using SewagePlantIMS.Models;
using SewagePlantIMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SewagePlantIMS.Controllers
{
    [LoginAttribute(isNeed = true)]
    public class ChemicalController : Controller
    {
        // GET: Chemical
        public ActionResult ChemicalDeviceList()
        {
            List<chemical_device> model = new List<chemical_device>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select id,cd_manufacturer,cd_name,cd_preserver from dm_chemical_device;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            chemical_device[] E = new chemical_device[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new chemical_device();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].cd_manufacturer = ds.Tables[0].Rows[mDr][1].ToString();
                E[mDr].cd_name = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].cd_preserver = ds.Tables[0].Rows[mDr][3].ToString();
                model.Add(E[mDr]);
            }
            con.Close();
            return View(model);
        }
        public void CreateQrCode()
        {
            string host = Request.Url.Host;
            var port = Request.Url.Port;
            string str = "http://" + host + ":" + port + "/Chemical/ShowChemicalDevice?id=" + Request.Form["qrcode"];
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select cd_name from dm_chemical_device where id = "+ Request.Form["qrcode"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            string cd_name = cmd.ExecuteScalar().ToString();
            con.Close();
            using (var memoryStream = QRCodeHelper.GetQRCode(str, 10))
            {
                System.Drawing.Image img = Image.FromStream(memoryStream);
                img = QRCodeHelper.AddTextToImg(img, cd_name, cd_name);
                
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                Response.ContentType = "application/octet-stream";
                //文件名+文件格式 （这里编码采用的是utf-8）
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(cd_name + ".png", System.Text.Encoding.UTF8));
                Response.BinaryWrite(ms.ToArray());
                ms.Dispose();
                img.Dispose();
            }
        }
        public ActionResult AddChemicalDevice()
        {
            return View();

        }
        public JavaScriptResult AddChemicalDevice_Post(chemical_device model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "insert into dm_chemical_device(cd_name,cd_model,cd_quantity,cd_num,cd_manufacturer,cd_preserver,cd_begin_time,cd_remark) values('" + model.cd_name + "','" + model.cd_model + "','" + model.cd_quantity + "','" + model.cd_num + "','" + model.cd_manufacturer + "','" + model.cd_preserver + "','" + model.cd_begin_time + "','" + model.cd_remark + "');";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                return JavaScript("swal_success();jump_ChemicalDeviceList();");
            else
                return JavaScript("swal_error();");
        }
        public ActionResult ModifyChemicalDevice()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select cd_name,cd_model,cd_quantity,cd_num,cd_manufacturer,cd_preserver,cd_begin_time,cd_remark from dm_chemical_device where id = " + Request.Form["id"] + ";";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds2 = new DataSet();
            da.Fill(ds2);
            chemical_device model = new chemical_device();
            model.cd_name = ds2.Tables[0].Rows[0][0].ToString();
            model.cd_model = ds2.Tables[0].Rows[0][1].ToString();
            model.cd_quantity = Convert.ToInt32(ds2.Tables[0].Rows[0][2]);
            model.cd_num = ds2.Tables[0].Rows[0][3].ToString();
            model.cd_manufacturer = ds2.Tables[0].Rows[0][4].ToString();
            model.cd_preserver = ds2.Tables[0].Rows[0][5].ToString();
            model.cd_begin_time = Convert.ToDateTime(ds2.Tables[0].Rows[0][6]);
            model.cd_remark = ds2.Tables[0].Rows[0][7].ToString();
            model.id = Convert.ToInt32(Request.Form["id"]);
            con.Close();
            return View(model);
        }
        public ActionResult ModifyChemicalDevice_post(chemical_device model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "update dm_chemical_device set cd_name = '" + model.cd_name + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_model = '" + model.cd_model + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_quantity = '" + model.cd_quantity + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_num = '" + model.cd_num + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_manufacturer = '" + model.cd_manufacturer + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_preserver = '" + model.cd_preserver + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_begin_time = '" + model.cd_begin_time + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_chemical_device set cd_remark = '" + model.cd_remark + "' where id = '" + Request.Form["id"] + "';";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
                return JavaScript("swal_success();jump_ChemicalDeviceList();");
            else
                return JavaScript("swal_error();");
        }
        [HttpGet]
        public ActionResult AddChemicalDevicePic(int id)
        {
            ViewBag.cd_id = id;
            List<chemical_device_pic> models = new List<chemical_device_pic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,cd_picurl from dm_chemical_device_pic where cd_id = " + ViewBag.cd_id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            chemical_device_pic[] tc = new chemical_device_pic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new chemical_device_pic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].cd_picurl = ds.Tables[0].Rows[mDr][1].ToString();

                models.Add(tc[mDr]);
            }
            con.Close();
            return View(models);
        }
        [HttpPost]
        public ActionResult AddChemicalDevicePic()
        {
            ViewBag.cd_id = Request.Form["id"];
            List<chemical_device_pic> models = new List<chemical_device_pic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,cd_picurl from dm_chemical_device_pic where cd_id = " + ViewBag.cd_id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            chemical_device_pic[] tc = new chemical_device_pic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new chemical_device_pic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].cd_picurl = ds.Tables[0].Rows[mDr][1].ToString();

                models.Add(tc[mDr]);
            }
            con.Close();
            return View(models);
        }
        public void AddChemicalDevicePicPost()
        {
            chemical_device_pic ep = new chemical_device_pic();
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
            string sql = "select max(id) from dm_chemical_device_pic";
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
            string filepath = Server.MapPath("/images/ChemicalDevicePic_Pre/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            ff.SaveAs(filepath);//在服务器上保存上传原图文件
                                //string[] readFile = System.IO.File.ReadAllLines(filepath);//读取txt文档存放在字符数组中
            string filepath2 = Server.MapPath("/images/ChemicalDevicePic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            CompressPic cp = new CompressPic();
            bool temp = cp.CompressImage(filepath, filepath2, 80, 150, true); //保存压缩完的文件到ChemicalDevicePic文件夹


            string sqlStr = "insert into dm_chemical_device_pic (cd_picname,cd_picurl,cd_addtime,cd_id) values('" + filename + "','" + "/images/ChemicalDevicePic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png" + "','" + DateTime.Now.ToString() + "','" + Request.Form["id"] + "')";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            if (check == 1)
            {
                Response.Write("<script>alert('添加图片成功！！');window.location.href='AddChemicalDevicePic?id=" + Request.Form["id"] + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('添加图片失败！,请手动联系管理员~');</script>");
            con.Close();
            //return RedirectToAction("Electrical_List"); //如果直接View就不会执行HttpGet的代码
            //return ElectricalPic();
        }
        public void DeleteChemicalDevicePic()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先查出url
            string pic_url = "select cd_picurl from dm_chemical_device_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd_url = new SqlCommand(pic_url, con);
            pic_url = cmd_url.ExecuteScalar().ToString();
            string sql = "select cd_id from dm_chemical_device_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd = new SqlCommand(sql, con);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            string sqlStr = "delete from dm_chemical_device_pic where id =  '" + Request.Form["del_id"] + "'";
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
            //删除原图
            string preurl = pic_url.Insert(25,"_Pre");
            filePath = Server.MapPath(preurl);//路径 
            file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }

            if (check == 1)
            {
                Response.Write("<script>alert('删除图片成功！！');window.location.href='AddChemicalDevicePic?id=" + id + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('删除失败！,请手动联系管理员~');</script>");
        }
        //化验设备展示页
        [LoginAttribute(isNeed = false)]
        [HttpGet]
        public ActionResult ShowChemicalDevice(string id)
        {
            List<ShowChemicalDevice> model = new List<ShowChemicalDevice>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select dm_chemical_device.id,cd_name,cd_model,cd_quantity,cd_num,cd_manufacturer,cd_preserver,cd_begin_time,cd_remark ,cd_picurl from dm_chemical_device,dm_chemical_device_pic where dm_chemical_device.id = '" + id + "'and  dm_chemical_device.id = dm_chemical_device_pic.cd_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ShowChemicalDevice[] E = new ShowChemicalDevice[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new ShowChemicalDevice();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].cd_name = ds.Tables[0].Rows[mDr][1].ToString();
                E[mDr].cd_model = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].cd_quantity = Convert.ToInt32(ds.Tables[0].Rows[mDr][3]);
                E[mDr].cd_num = ds.Tables[0].Rows[mDr][4].ToString();
                E[mDr].cd_manufacturer = ds.Tables[0].Rows[mDr][5].ToString();
                E[mDr].cd_preserver = ds.Tables[0].Rows[mDr][6].ToString();
                E[mDr].cd_begin_time = Convert.ToDateTime(ds.Tables[0].Rows[mDr][7]);
                E[mDr].cd_remark = ds.Tables[0].Rows[mDr][8].ToString();
                E[mDr].cd_picurl = ds.Tables[0].Rows[mDr][9].ToString();
                model.Add(E[mDr]);
            }
            con.Close();
            return View(model);
        }
        [HttpPost]
        public ActionResult ShowChemicalDevice()
        {
            List<ShowChemicalDevice> model = new List<ShowChemicalDevice>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select dm_chemical_device.id,cd_name,cd_model,cd_quantity,cd_num,cd_manufacturer,cd_preserver,cd_begin_time,cd_remark ,cd_picurl from dm_chemical_device,dm_chemical_device_pic where dm_chemical_device.id = '" + Request.Form["show"] + "'and  dm_chemical_device.id = dm_chemical_device_pic.cd_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ShowChemicalDevice[] E = new ShowChemicalDevice[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new ShowChemicalDevice();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].cd_name = ds.Tables[0].Rows[mDr][1].ToString();
                E[mDr].cd_model = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].cd_quantity = Convert.ToInt32(ds.Tables[0].Rows[mDr][3]);
                E[mDr].cd_num = ds.Tables[0].Rows[mDr][4].ToString();
                E[mDr].cd_manufacturer = ds.Tables[0].Rows[mDr][5].ToString();
                E[mDr].cd_preserver = ds.Tables[0].Rows[mDr][6].ToString();
                E[mDr].cd_begin_time = Convert.ToDateTime(ds.Tables[0].Rows[mDr][7]);
                E[mDr].cd_remark = ds.Tables[0].Rows[mDr][8].ToString();
                E[mDr].cd_picurl = ds.Tables[0].Rows[mDr][9].ToString();
                model.Add(E[mDr]);
            }
            con.Close();
            return View(model);
        }
        public JavaScriptResult DeleteChemicalDevice()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先删除对应的图片
            string del_pic = "select cd_picurl from dm_chemical_device_pic where cd_id = " + Request.Form["del"];
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
            //删除图片原图
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {

                string filePath = Server.MapPath(ds.Tables[0].Rows[mDr][0].ToString().Insert(25,"_Pre"));
                FileInfo file2 = new FileInfo(filePath);
                if (file2.Exists)
                {
                    file2.Delete();
                }
            }
            //最后删除相应的数据库数据
            string sqlStr = "delete from dm_chemical_device_pic where cd_id = " + Request.Form["del"] + "; delete from dm_chemical_device where id = " + Request.Form["del"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
            {
                return JavaScript("swal_success();jump_ChemicalDeviceList()");
            }
            else
            {
                return JavaScript("swal_error();");
            }
        }
    }
}