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
    public class DeviceController : Controller
    {
        // GET: Device
        //设备列表呈现
        public ActionResult DeviceList()
        {
            List<Device> model = new List<Device>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select id,technology_id,class_id,title,brand_id from dm_device;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            Device[] E = new Device[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new Device();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].technology_id = Convert.ToInt32(ds.Tables[0].Rows[mDr][1]);
                E[mDr].class_id = Convert.ToInt32(ds.Tables[0].Rows[mDr][2]);
                E[mDr].title = ds.Tables[0].Rows[mDr][3].ToString();
                E[mDr].brand_id = Convert.ToInt32(ds.Tables[0].Rows[mDr][4]);
                model.Add(E[mDr]);
            }

            List<string> technology_name = new List<string>();
            List<string> class_name = new List<string>();
            List<string> brand_name = new List<string>();
            SqlCommand cmd = new SqlCommand();
            foreach (var data in model)
            {
                sqlStr = "select title from dm_technology where id = " + data.technology_id + ";";
                cmd = new SqlCommand(sqlStr, con);
                technology_name.Add(cmd.ExecuteScalar().ToString());
                sqlStr = "select title from dm_device_class where id = " + data.class_id + ";";
                cmd = new SqlCommand(sqlStr, con);
                class_name.Add(cmd.ExecuteScalar().ToString());
                sqlStr = "select title from dm_supplier_brand where id = " + data.brand_id + ";";
                cmd = new SqlCommand(sqlStr, con);
                brand_name.Add(cmd.ExecuteScalar().ToString());
            }
            ViewBag.technology_name = technology_name;
            ViewBag.class_name = class_name;
            ViewBag.brand_name = brand_name;
            con.Close();
            return View(model);
        }
        //新增设备信息
        public ActionResult AddDevice()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有分类
            string sqlStr = "select id,title from dm_device_class where id not in (select distinct parent_id from dm_device_class);";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_class = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_class.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_class = list_class;
            //2)查出所有工艺段
            sqlStr = "select id,title from dm_technology;";
            da = new SqlDataAdapter(sqlStr, con);
            ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_tecnology = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_tecnology.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_tecnology = list_tecnology;
            //3)查出所有的品牌
            sqlStr = "select id,title from dm_supplier_brand;";
            da = new SqlDataAdapter(sqlStr, con);
            ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_brand = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_brand.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_brand = list_brand;
            //4)设置一下状态
            List<SelectListItem> state = new List<SelectListItem>();
            state.Add(new SelectListItem() { Value = "1", Text = "正常" });
            state.Add(new SelectListItem() { Value = "0", Text = "异常" });
            ViewBag.state = state;
            con.Close();
            return View();
        }
        //提交设备信息
        [HttpPost]
        public JavaScriptResult AddDevice_post(Device model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "insert into dm_device(title,alias,class_id,technology_id,brand_id,state,device_power,device_model,summary,purchase_date,add_date,add_user_id,add_user_name) values('" + model.title + "','" + model.alias + "'," + model.class_id + "," + model.technology_id + "," + model.brand_id + "," + model.state + ",'" + model.device_power + "','" + model.device_model + "','" + model.summary + "','" + model.purchase_date + "','" + DateTime.Now + "'," + Session["user_id"] + ",'" + Session["real_name"] + "')";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                return JavaScript("swal_success();jump_DeviceList();");
            else
                return JavaScript("swal_error();");
        }
        //修改设备信息
        public ActionResult ModifyDevice()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            Device model = new Device();
            //0)查出当前设备的信息
            string sqlinfo = "select id,title,alias,class_id,technology_id,brand_id,state,device_power,device_model,summary,purchase_date from dm_device where id = " + Request.Form["id"] + ";";
            SqlCommand cmd = new SqlCommand(sqlinfo, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.title = reader["title"].ToString();
                model.alias = reader["alias"].ToString();
                model.class_id = Convert.ToInt32(reader["class_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.brand_id = Convert.ToInt32(reader["brand_id"]);
                model.state = Convert.ToInt32(reader["state"]);
                model.device_power = reader["device_power"].ToString();
                model.device_model = reader["device_model"].ToString();
                model.summary = reader["summary"].ToString();
                if (reader["purchase_date"] != DBNull.Value) model.purchase_date = Convert.ToDateTime(reader["purchase_date"]);   //特殊情况
            }
            reader.Close();
            //1)查出所有分类
            string sqlStr = "select id,title from dm_device_class where id not in (select distinct parent_id from dm_device_class);";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_class = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_class.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_class = list_class;
            //2)查出所有工艺段
            sqlStr = "select id,title from dm_technology;";
            da = new SqlDataAdapter(sqlStr, con);
            ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_tecnology = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_tecnology.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_tecnology = list_tecnology;
            //3)查出所有的品牌
            sqlStr = "select id,title from dm_supplier_brand;";
            da = new SqlDataAdapter(sqlStr, con);
            ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_brand = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_brand.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_brand = list_brand;
            //4)设置一下状态
            List<SelectListItem> state = new List<SelectListItem>();
            state.Add(new SelectListItem() { Value = "1", Text = "正常" });
            state.Add(new SelectListItem() { Value = "0", Text = "异常" });
            ViewBag.state = state;
            con.Close();
            return View(model);
        }
        public JavaScriptResult ModifyDevice_post(Device model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "update dm_device set title = '" + model.title + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set alias = '" + model.alias + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set class_id = '" + model.class_id + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set technology_id = '" + model.technology_id + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set brand_id = '" + model.brand_id + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set state = '" + model.state + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set device_power = '" + model.device_power + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set device_model = '" + model.device_model + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set summary = '" + model.summary + "' where id = '" + Request.Form["id"] + "';" +
                            "update dm_device set purchase_date = '" + model.purchase_date + "' where id = '" + Request.Form["id"] + "';";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
                return JavaScript("swal_success();jump_DeviceList();");
            else
                return JavaScript("swal_error();");
        }
        //添加设备图片
        public ActionResult AddDevicePic(int id)
        {
            ViewBag.id = id;
            List<DevicePic> models = new List<DevicePic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,pic_url from dm_device_pic where device_id = " + ViewBag.id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            DevicePic[] tc = new DevicePic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new DevicePic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][1].ToString();

                models.Add(tc[mDr]);
            }
            con.Close();
            return View(models);
        }
        [HttpPost]
        public ActionResult AddDevicePic()
        {
            ViewBag.id = Request.Form["id"];
            List<DevicePic> models = new List<DevicePic>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            string sqlStr = "select id,pic_url from dm_device_pic where device_id = " + ViewBag.id + "; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            DevicePic[] tc = new DevicePic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new DevicePic();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][1].ToString();

                models.Add(tc[mDr]);
            }
            con.Close();
            return View(models);
        }
        public void AddDevicePicPost()
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
            string sql = "select max(id) from dm_device_pic";
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
            string filepath = Server.MapPath("/images/DevicePic_Pre/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            ff.SaveAs(filepath);//在服务器上保存上传原图文件
                                //string[] readFile = System.IO.File.ReadAllLines(filepath);//读取txt文档存放在字符数组中
            string filepath2 = Server.MapPath("/images/DevicePic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png");
            CompressPic cp = new CompressPic();
            bool temp = cp.CompressImage(filepath, filepath2, 80, 150, true); //保存压缩完的文件到ChemicalDevicePic文件夹


            string sqlStr = "insert into dm_device_pic (title,pic_url,add_date,device_id) values('" + filename + "','" + "/images/DevicePic/" + filename + "_" + Request.Form["id"] + "_" + max_id.ToString() + ".png" + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + Request.Form["id"] + "')";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            if (check == 1)
            {
                Response.Write("<script>alert('添加图片成功！！');window.location.href='AddDevicePic?id=" + Request.Form["id"] + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('添加图片失败！,请手动联系管理员~');</script>");
            con.Close();
            //return RedirectToAction("Electrical_List"); //如果直接View就不会执行HttpGet的代码
            //return ElectricalPic();
        }
        //删除设备图片
        public void DeleteDevicePic()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先查出url
            string pic_url = "select pic_url from dm_device_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd_url = new SqlCommand(pic_url, con);
            pic_url = cmd_url.ExecuteScalar().ToString();
            string sql = "select device_id from dm_device_pic  where id =  '" + Request.Form["del_id"] + "'";
            SqlCommand cmd = new SqlCommand(sql, con);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            string sqlStr = "delete from dm_device_pic where id =  '" + Request.Form["del_id"] + "'";
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
            string preurl = pic_url.Insert(17, "_Pre");
            filePath = Server.MapPath(preurl);//路径 
            file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }

            if (check == 1)
            {
                Response.Write("<script>alert('删除图片成功！！');window.location.href='AddDevicePic?id=" + id + " '   ;</script>");
            }
            else
                Response.Write("<script>alert('删除失败！,请手动联系管理员~');</script>");
        }
        //设备展示页
        [LoginAttribute(isNeed = false)]
        [HttpGet]
        public ActionResult ShowDevice(string id)
        {
            Device model = new Device();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select id,title,class_id,technology_id,state,brand_id,device_power,device_model,summary,purchase_date from dm_device where id = " + id + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.title = reader["title"].ToString();
                model.class_id = Convert.ToInt32(reader["class_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.brand_id = Convert.ToInt32(reader["brand_id"]);
                model.state = Convert.ToInt32(reader["state"]);
                model.device_power = reader["device_power"].ToString();
                model.device_model = reader["device_model"].ToString();
                model.summary = reader["summary"].ToString();
                if (reader["purchase_date"] != DBNull.Value) model.purchase_date = Convert.ToDateTime(reader["purchase_date"]);   //特殊情况
            }
            reader.Close();
            //0)查出所有图片地址
            List<DevicePic> dp = new List<DevicePic>();
            DevicePic dd;
            sql = "select id,pic_url from dm_device_pic where device_id = " + model.id + ";";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dd = new DevicePic();
                dd.id = Convert.ToInt32(reader["id"]);
                dd.pic_url = reader["pic_url"].ToString();
                dp.Add(dd);
            }
            ViewBag.device_pic = dp;
            reader.Close();
            //1)查出分类中文名
            string sqlStr = "select title from dm_device_class where id = " + model.class_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_class = cmd.ExecuteScalar().ToString();
            //2)查出所有工艺段
            sqlStr = "select title from dm_technology where id = " + model.technology_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_tecnology = cmd.ExecuteScalar().ToString();
            //3)查出所有的品牌
            sqlStr = "select title from dm_supplier_brand where id = " + model.brand_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_brand = cmd.ExecuteScalar().ToString();
            //4)设置一下状态
            if (model.state == 1)
                ViewBag.state = "正常";
            else
                ViewBag.state = "异常";
            con.Close();
            return View(model);

        }
        [HttpPost]
        public ActionResult ShowDevice()
        {
            Device model = new Device();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select id,title,class_id,technology_id,state,brand_id,device_power,device_model,summary,purchase_date from dm_device where id = " + Request.Form["show"] + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.title = reader["title"].ToString();
                model.class_id = Convert.ToInt32(reader["class_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.brand_id = Convert.ToInt32(reader["brand_id"]);
                model.state = Convert.ToInt32(reader["state"]);
                model.device_power = reader["device_power"].ToString();
                model.device_model = reader["device_model"].ToString();
                model.summary = reader["summary"].ToString();
                if (reader["purchase_date"] != DBNull.Value) model.purchase_date = Convert.ToDateTime(reader["purchase_date"]);   //特殊情况
            }
            reader.Close();
            //0)查出所有图片地址
            List<DevicePic> dp = new List<DevicePic>();
            DevicePic dd;
            sql = "select id,pic_url from dm_device_pic where device_id = " + model.id + ";";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dd = new DevicePic();
                dd.id = Convert.ToInt32(reader["id"]);
                dd.pic_url = reader["pic_url"].ToString();
                dp.Add(dd);
            }
            ViewBag.device_pic = dp;
            reader.Close();
            //1)查出分类中文名
            string sqlStr = "select title from dm_device_class where id = " + model.class_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_class = cmd.ExecuteScalar().ToString();
            //2)查出所有工艺段
            sqlStr = "select title from dm_technology where id = " + model.technology_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_tecnology = cmd.ExecuteScalar().ToString();
            //3)查出所有的品牌
            sqlStr = "select title from dm_supplier_brand where id = " + model.brand_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_brand = cmd.ExecuteScalar().ToString();
            //4)设置一下状态
            if (model.state == 1)
                ViewBag.state = "正常";
            else
                ViewBag.state = "异常";
            con.Close();
            return View(model);
        }
        //二维码下载
        public void CreateQrCode()
        {
            string host = Request.Url.Host;
            var port = Request.Url.Port;
            string str = "http://" + host + ":" + port + "/Device/ShowDevice?id=" + Request.Form["qrcode"];
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select title from dm_device where id = " + Request.Form["qrcode"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            string name = cmd.ExecuteScalar().ToString();
            con.Close();
            using (var memoryStream = QRCodeHelper.GetQRCode(str, 10))
            {
                System.Drawing.Image img = Image.FromStream(memoryStream);
                img = QRCodeHelper.AddTextToImg(img, name, name);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                Response.ContentType = "application/octet-stream";
                //文件名+文件格式 （这里编码采用的是utf-8）
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(name + ".png", System.Text.Encoding.UTF8));
                Response.BinaryWrite(ms.ToArray());
                ms.Dispose();
                img.Dispose();
            }
        }
        //删除设备
        public JavaScriptResult DeleteDevice()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先删除对应的图片
            string del_pic = "select pic_url from dm_device_pic where device_id = " + Request.Form["del"];
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

                string filePath = Server.MapPath(ds.Tables[0].Rows[mDr][0].ToString().Insert(17, "_Pre"));
                FileInfo file2 = new FileInfo(filePath);
                if (file2.Exists)
                {
                    file2.Delete();
                }
            }
            //最后删除相应的数据库数据
            string sqlStr = "delete from dm_device_pic where device_id = " + Request.Form["del"] + "; delete from dm_device where id = " + Request.Form["del"] + ";";
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
            {
                return JavaScript("swal_success();jump_DeviceList()");
            }
            else
            {
                return JavaScript("swal_error();");
            }
        }

        ///////////下面是和维修有关的内容//////////////////
        public ActionResult DeviceRepair()
        {

            return View();
        }
        //将sql查询的内容转化为JSON格式
        public string SelectDeviceRepairList()
        {
            /*保存一下，这个是读取json文件再通过后台传输到前台的代码  
             "msg": "",
             "count": 15,
             这两段没有的话不要紧
            string jsonfile = Server.MapPath("user.json");

            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonfile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    //object JSONObj = JsonConvert.SerializeObject(o);
                    return o.ToString();
                }
            }*/
            //建立一个字符串,严格按照规定的json格式租合
            /*string s = "{\"name\":\"王小明\",\"age\":\"26\",\"sex\":\"男\",\"graduate\":\"加利顿大学7987879798\"},";
            s = "{\"code\": 0,\"data\": [" + s;
            s = s + "]}";
            //建立一个json对象
            JObject json = (JObject)JsonConvert.DeserializeObject(s.ToString());
            return json.ToString();*/
            //建立一个字符串，用来组合成json字符串
            string str = "";
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();

            //先将对应的设备ID取出生成字典备用
            string sql = "select ID,title from dm_device";
            Dictionary<int, string> dic_device = new Dictionary<int, string>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_device.Add(Convert.ToInt32(reader["id"]), reader["title"].ToString());
            }
            reader.Close();
            //再将对应的工艺段取出来
            sql = "select ID,title from dm_technology;";
            Dictionary<int, string> dic_technology = new Dictionary<int, string>();
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_technology.Add(Convert.ToInt32(reader["id"]), reader["title"].ToString());
            }
            reader.Close();
            //最后把设备维修表给查询出来
            //对年月份做一个判断修改,为的是显示当前的年份和月份
            string year, month;
            if (Request["year"] == null) year = DateTime.Now.Year.ToString(); else year = Request["year"].ToString();
            if (Request["month"] == null) month = DateTime.Now.Month.ToString(); else month = Request["month"].ToString();
            sql = "select id,device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark from dm_device_repair where repair_title like '%" + Request["keyword"] + "%' and CONVERT(VARCHAR,Year(repair_date)) like '%" + year + "%' and Right(100+Month(repair_date),2) like '%" + month + "%' order by repair_finsh desc;";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            //设置一个变量count来记录数据条数
            int count = 0;
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                str += "{\"id\":\"" + reader["id"].ToString() + "\",";
                str += "\"device_id\":\"" + dic_device[Convert.ToInt32(reader["device_id"])] + "\",";
                str += "\"user_id\":\"" + reader["user_id"].ToString() + "\",";
                str += "\"technology_id\":\"" + dic_technology[Convert.ToInt32(reader["technology_id"])] + "\",";
                str += "\"repair_date\":\"" + reader["repair_date"].ToString() + "\",";
                str += "\"repair_finsh\":\"" + reader["repair_finsh"].ToString() + "\",";
                str += "\"repair_class\":\"" + reader["repair_class"].ToString() + "\",";
                str += "\"repair_title\":\"" + reader["repair_title"].ToString() + "\",";
                str += "\"repair_nums\":\"" + reader["repair_nums"].ToString() + "\",";
                str += "\"repair_reasons\":\"" + reader["repair_reasons"].ToString() + "\",";
                str += "\"repair_conclusion\":\"" + reader["repair_conclusion"].ToString() + "\",";
                str += "\"repair_join\":\"" + reader["repair_join"].ToString() + "\",";
                str += "\"repair_consumption\":\"" + reader["repair_consumption"].ToString() + "\",";
                str += "\"repair_mark\":\"" + reader["repair_mark"].ToString() + "\"},";
                count += 1;
            }
            str = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + str;
            str = str + "]}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            reader.Close();
            con.Close();
            return json.ToString();
        }
        //一个测试的方法，暂时存放在这里
        public string test()
        {
            return Request.Form["data"];
        }
        //新增设备维修记录
        [HttpGet]
        public ActionResult AddDeviceRepair()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //2)设置一下维修类别
            List<SelectListItem> state = new List<SelectListItem>();
            state.Add(new SelectListItem() { Value = "常规维护", Text = "常规维护" });
            state.Add(new SelectListItem() { Value = "故障检修", Text = "故障检修" });
            state.Add(new SelectListItem() { Value = "突发维修", Text = "突发维修" });
            state.Add(new SelectListItem() { Value = "改装改造", Text = "改装改造" });
            state.Add(new SelectListItem() { Value = "其他", Text = "其他" });
            ViewBag.state = state;
            //2.1）设置一下是否审批
            List<SelectListItem> isapproval = new List<SelectListItem>();
            isapproval.Add(new SelectListItem() { Value = "0", Text = "未审批" });
            isapproval.Add(new SelectListItem() { Value = "1", Text = "已审批" });
            ViewBag.isapproval = isapproval;
            //2.2）设置一下是否完工
            List<SelectListItem> isover = new List<SelectListItem>();
            isover.Add(new SelectListItem() { Value = "0", Text = "未完工" });
            isover.Add(new SelectListItem() { Value = "1", Text = "已完工" });
            ViewBag.isover = isover;
            con.Close();
            return View();
        }
        public JavaScriptResult AddDeviceRepair_Post(DeviceRepair model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查出所有的设备id及其对应的工艺段ID放入字典
            string sql = "select ID,technology_id from dm_device";
            Dictionary<int, int> dic_id = new Dictionary<int, int>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_id.Add(Convert.ToInt32(reader["id"]), Convert.ToInt32(reader["technology_id"]));
            }
            reader.Close();
            //插入对应的数据
            sql = "insert into dm_device_repair(device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark,repair_begin,repair_starts,repair_consume,manager_opinion,isapproval,isover) values(" +
                    model.device_id + "," + Session["user_id"] + "," + dic_id[model.device_id] + ",'" + model.repair_date + "','" + model.repair_finsh + "','" + model.repair_class + "','" + model.repair_title + "'," + model.repair_nums + ",'" + model.repair_reasons + "','" + model.repair_conclusion + "','" + model.repair_join + "','" + model.repair_consumption + "','" + model.repair_mark + "','" + model.repair_begin + "','" + model.repair_starts + "'," + model.repair_consume + ",'" + model.manager_opinion + "'," + model.isapproval + "," + model.isover + ")";
            cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
                return JavaScript("swal_success();jump_DeviceRepair();");
            else
                return JavaScript("swal_error();");
        }
        //修改维修内容
        [HttpGet]
        public ActionResult ModifyDeviceRepair(string id)
        {
            ViewBag.id = id;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //2)设置一下维修类别
            List<SelectListItem> state = new List<SelectListItem>();
            state.Add(new SelectListItem() { Value = "常规维护", Text = "常规维护" });
            state.Add(new SelectListItem() { Value = "故障检修", Text = "故障检修" });
            state.Add(new SelectListItem() { Value = "突发维修", Text = "突发维修" });
            state.Add(new SelectListItem() { Value = "改装改造", Text = "改装改造" });
            state.Add(new SelectListItem() { Value = "其他", Text = "其他" });
            ViewBag.state = state;
            //2.1）设置一下是否审批
            List<SelectListItem> isapproval = new List<SelectListItem>();
            isapproval.Add(new SelectListItem() { Value = "0", Text = "未审批" });
            isapproval.Add(new SelectListItem() { Value = "1", Text = "已审批" });
            ViewBag.isapproval = isapproval;
            //2.2）设置一下是否完工
            List<SelectListItem> isover = new List<SelectListItem>();
            isover.Add(new SelectListItem() { Value = "0", Text = "未完工" });
            isover.Add(new SelectListItem() { Value = "1", Text = "已完工" });
            ViewBag.isover = isover;
            //3)查出对应ID的设备维修详细内容
            sqlStr = "select id,device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark,repair_begin,repair_starts,repair_consume,manager_opinion,isapproval,isover from dm_device_repair where id = " + id + ";";
            DeviceRepair dr = new DeviceRepair();
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dr.id = Convert.ToInt32(reader["id"]);
                dr.device_id = Convert.ToInt32(reader["device_id"]);
                dr.user_id = Convert.ToInt32(reader["user_id"]);
                dr.technology_id = Convert.ToInt32(reader["technology_id"]);
                dr.repair_date = Convert.ToDateTime(reader["repair_date"]);
                dr.repair_finsh = Convert.ToDateTime(reader["repair_finsh"]);
                dr.repair_class = reader["repair_class"].ToString();
                //去除空格
                dr.repair_class = dr.repair_class.Trim();
                dr.repair_title = reader["repair_title"].ToString();
                dr.repair_nums = Convert.ToInt32(reader["repair_nums"]);
                dr.repair_reasons = reader["repair_reasons"].ToString();
                dr.repair_conclusion = reader["repair_conclusion"].ToString();
                dr.repair_join = reader["repair_join"].ToString();
                dr.repair_consumption = reader["repair_consumption"].ToString();
                dr.repair_mark = reader["repair_mark"].ToString();
                if(reader["repair_begin"]!=DBNull.Value) dr.repair_begin = Convert.ToDateTime(reader["repair_begin"]);
                if (reader["repair_starts"] != DBNull.Value) dr.repair_starts = Convert.ToDateTime(reader["repair_starts"]);
                if (reader["repair_consume"] != DBNull.Value) dr.repair_consume = double.Parse(reader["repair_consume"].ToString()); //不然会有很多小数点。
                if (reader["repair_starts"] != DBNull.Value) dr.manager_opinion = reader["manager_opinion"].ToString();
                if (reader["isapproval"] != DBNull.Value) dr.isapproval = Convert.ToInt32(reader["isapproval"]);
                if (reader["isover"] != DBNull.Value) dr.isover = Convert.ToInt32(reader["isover"]);
            }
            reader.Close();
            con.Close();
            return View(dr);
        }
        public JavaScriptResult ModifyDeviceRepair_Post(DeviceRepair model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查出所有的设备id及其对应的工艺段ID放入字典
            string sql = "select ID,technology_id from dm_device";
            Dictionary<int, int> dic_id = new Dictionary<int, int>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_id.Add(Convert.ToInt32(reader["id"]), Convert.ToInt32(reader["technology_id"]));
            }
            reader.Close();
            sql = "update dm_device_repair set device_id = " + model.device_id + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set user_id = " + Session["user_id"] + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set technology_id = " + dic_id[model.device_id] + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_date = '" + model.repair_date + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_finsh = '" + model.repair_finsh + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_class = '" + model.repair_class + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_title = '" + model.repair_title + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_nums = " + model.repair_nums + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_reasons = '" + model.repair_reasons + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_conclusion = '" + model.repair_conclusion + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_join = '" + model.repair_join + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_consumption = '" + model.repair_consumption + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_mark = '" + model.repair_mark + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_begin = '" + model.repair_begin + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_starts = '" + model.repair_starts + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set repair_consume = " + model.repair_consume + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set manager_opinion = '" + model.manager_opinion + "' where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set isapproval = " + model.isapproval + " where id = " + Request.Form["idd"] + ";" +
                  "update dm_device_repair set isover = '" + model.isover + "' where id = " + Request.Form["idd"] + ";";
            cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
                return JavaScript("swal_success();jump_DeviceRepair();");
            else
                return JavaScript("swal_error();");
        }
        //维修图片上传
        public ActionResult DeviceRepairPic(string id)
        {
            //获取传输过来的ID
            ViewBag.id = id;
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select pic_url,describe,id from dm_device_repair_pic where type = 1 and repair_id = " + id + ";";
            List<DeviceRepairPic> models = new List<DeviceRepairPic>();
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            DeviceRepairPic[] tc = new DeviceRepairPic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new DeviceRepairPic();
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][0].ToString();
                tc[mDr].describe = ds.Tables[0].Rows[mDr][1].ToString();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][2]);
                models.Add(tc[mDr]);
            }
            return View(models);
        }
        public string DeviceRepairPic_Post()
        {

            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            //获取文件名后缀
            string extName = Path.GetExtension(file.FileName).ToLower();
            //获取保存目录的物理路径
            if (System.IO.Directory.Exists(Server.MapPath("/images/DeviceRepairPic/")) == false)//如果不存在就创建images文件夹
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("/images/DeviceRepairPic/"));
            }
            string path = Server.MapPath("/images/DeviceRepairPic/"); //path为某个文件夹的绝对路径，不要直接保存到数据库
                                                                      //    string path = "F:\\TgeoSmart\\Image\\";
                                                                      //生成新文件的名称，guid保证某一时刻内图片名唯一（文件不会被覆盖）
            string fileNewName = Guid.NewGuid().ToString();
            //获取前端图片描述
            string describe = Request["describe"];
            string id = Request["id"];
            string ImageUrl = path + fileNewName + "_cp" + extName;
            string ImageUrl2 = path + fileNewName + extName; //压缩备用
            //连接数据库把图片地址还有描述等插入
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "insert into dm_device_repair_pic(pic_url,describe,add_date,repair_id,type) values('" + "/images/DeviceRepairPic/" + fileNewName + extName + "','" + describe + "','" + DateTime.Now.ToString() + "'," + id +",1" +  ");";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
            {
                if (extName == ".jpg" || extName == ".png")
                {
                    //SaveAs将文件保存到指定文件夹中
                    file.SaveAs(ImageUrl);
                    //如果是JPG或者PNG文件压缩一下

                    CompressPic cp = new CompressPic();
                    bool temp = cp.CompressImage(ImageUrl, ImageUrl2, 80, 150, true);
                    //删除未压缩图片
                    FileInfo del_file = new FileInfo(ImageUrl);
                    if (del_file.Exists)
                    {
                        del_file.Delete();
                    }
                }
                else
                {
                    file.SaveAs(ImageUrl2);
                }
                //建立JSON字符串返回
                string str = "\"src\": " + id;
                str = "{\"code\": 0,\"data\": {" + str;
                str = str + "}}";
                JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                return json.ToString();
            }
            else
            {
                string str = "";
                JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                return json.ToString();
            }



        }
        //删除对应图片
        public JavaScriptResult DeleteDeviceRepairPic()
        {
            //先查询出该图片的地址
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先查出url
            string pic_url = "select pic_url from dm_device_repair_pic  where id =  '" + Request.Form["id"] + "';";
            SqlCommand cmd_url = new SqlCommand(pic_url, con);
            pic_url = cmd_url.ExecuteScalar().ToString();
            //查出对应的repair_id
            string repair_id = "select repair_id from dm_device_repair_pic where id = '" + Request.Form["id"] + "';";
            cmd_url = new SqlCommand(repair_id, con);
            repair_id = cmd_url.ExecuteScalar().ToString();
            //删除对应的实体文件
            string filePath = Server.MapPath(pic_url);//路径 
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
            //删除对应的数据库记录
            string sql = "delete from dm_device_repair_pic where id = '" + Request.Form["id"] + "';";
            cmd_url = new SqlCommand(sql, con);
            //检查执行是否成功
            int check = cmd_url.ExecuteNonQuery();
            con.Close();
            if (check == 1)
            {
                return JavaScript("del_pic_success(" + repair_id + ");");
            }
            else
            {
                return JavaScript("del_pic_error(" + repair_id + ");");
            }
        }
        //修改图片对应的描述
        public JavaScriptResult ModifyDeviceRepairPicDescribe()
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //写出查询语句
            string sql = "update dm_device_repair_pic set describe = '" + Request.Form["newtitle"] + "' where id = " + Request.Form["pic_id"] + " ;";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            //再把repair_id 拿出来等等返回用
            sql = "select repair_id from dm_device_repair_pic where id = " + Request.Form["pic_id"] + ";";
            cmd = new SqlCommand(sql, con);
            int idd = Convert.ToInt32(cmd.ExecuteScalar());
            con.Close();
            if (check == 1)
            {
                return JavaScript("del_pic_success(" + idd + ");");
            }
            else
            {
                return JavaScript("del_pic_error(" + idd + ");");
            }
        }
        //上传维修故障图片
        public ActionResult DeviceRepairFaultPic(string id)
        {
            //获取传输过来的ID
            ViewBag.id = id;
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select pic_url,describe,id from dm_device_repair_pic where type = 0 and repair_id = " + id + ";";
            List<DeviceRepairPic> models = new List<DeviceRepairPic>();
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            DeviceRepairPic[] tc = new DeviceRepairPic[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                tc[mDr] = new DeviceRepairPic();
                tc[mDr].pic_url = ds.Tables[0].Rows[mDr][0].ToString();
                tc[mDr].describe = ds.Tables[0].Rows[mDr][1].ToString();
                tc[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][2]);
                models.Add(tc[mDr]);
            }
            return View(models);
        }
        public string DeviceRepairFaultPic_Post()
        {

            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            //获取文件名后缀
            string extName = Path.GetExtension(file.FileName).ToLower();
            //获取保存目录的物理路径
            if (System.IO.Directory.Exists(Server.MapPath("/images/DeviceRepairPic/")) == false)//如果不存在就创建images文件夹
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("/images/DeviceRepairPic/"));
            }
            string path = Server.MapPath("/images/DeviceRepairPic/"); //path为某个文件夹的绝对路径，不要直接保存到数据库
                                                                      //    string path = "F:\\TgeoSmart\\Image\\";
                                                                      //生成新文件的名称，guid保证某一时刻内图片名唯一（文件不会被覆盖）
            string fileNewName = Guid.NewGuid().ToString();
            //获取前端图片描述
            string describe = Request["describe"];
            string id = Request["id"];
            string ImageUrl = path + fileNewName + "_cp" + extName;
            string ImageUrl2 = path + fileNewName + extName; //压缩备用
            //连接数据库把图片地址还有描述等插入
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "insert into dm_device_repair_pic(pic_url,describe,add_date,repair_id,type) values('" + "/images/DeviceRepairPic/" + fileNewName + extName + "','" + describe + "','" + DateTime.Now.ToString() + "'," + id + ",0" + ");";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
            {
                if (extName == ".jpg" || extName == ".png")
                {
                    //SaveAs将文件保存到指定文件夹中
                    file.SaveAs(ImageUrl);
                    //如果是JPG或者PNG文件压缩一下

                    CompressPic cp = new CompressPic();
                    bool temp = cp.CompressImage(ImageUrl, ImageUrl2, 80, 150, true);
                    //删除未压缩图片
                    FileInfo del_file = new FileInfo(ImageUrl);
                    if (del_file.Exists)
                    {
                        del_file.Delete();
                    }
                }
                else
                {
                    file.SaveAs(ImageUrl2);
                }
                //建立JSON字符串返回
                string str = "\"src\": " + id;
                str = "{\"code\": 0,\"data\": {" + str;
                str = str + "}}";
                JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                return json.ToString();
            }
            else
            {
                string str = "";
                JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
                return json.ToString();
            }



        }
        //删除与维修记录有关一切信息
        public string DeleteDeviceRepair(string id)
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查询出所有图片的url
            string sql = "select pic_url from dm_device_repair_pic where repair_id = " + id + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            string filePath;
            FileInfo file;
            while (reader.Read())
            {
                //删除对应的实体文件
                filePath = Server.MapPath(reader["pic_url"].ToString());//路径 
                file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
            reader.Close();
            //删除维修图片和维修记录
            sql = "delete from dm_device_repair_pic where repair_id = " + id + ";" + "delete from dm_device_repair where id = " + id + "; ";
            cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            if (check == 2)
            {
                return "";
            }
            else
            {
                return "";
            }

        }
        //导出维修审批表
        public void OutputDeiveRepairApprovalExcel(string id)
        {

        }
        //导出维修记录表
        public void OutputDeiveRepairExcel(string id)
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查询出对应的维修数据
            string sql = "select * from dm_device_repair where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            DeviceRepair model = new DeviceRepair();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.repair_date = Convert.ToDateTime(reader["repair_date"]);
                model.repair_finsh = Convert.ToDateTime(reader["repair_finsh"]);
                model.repair_class = reader["repair_class"].ToString();
                model.repair_title = reader["repair_title"].ToString();
                model.repair_nums = Convert.ToInt32(reader["repair_nums"]);
                model.repair_reasons = reader["repair_reasons"].ToString();
                model.repair_conclusion = reader["repair_conclusion"].ToString();
                model.repair_join = reader["repair_join"].ToString();
                model.repair_consumption = reader["repair_consumption"].ToString();
                model.repair_mark = reader["repair_mark"].ToString();
            }
            reader.Close();
            //查询出对应的设备名称
            sql = "select title from dm_device where id = " + model.device_id;
            cmd = new SqlCommand(sql, con);
            string device_name = cmd.ExecuteScalar().ToString();
            //查询出对应工艺段的名称
            sql = "select title from dm_technology where id = " + model.technology_id;
            cmd = new SqlCommand(sql, con);
            string technology_name = cmd.ExecuteScalar().ToString();
            //查询出对应的图片（最多四张）
            sql = "select pic_url from dm_device_repair_pic where type = 1 and repair_id = " + model.id;
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            List<string> pic_url = new List<string>();


            while (reader.Read())
            {
                pic_url.Add(Server.MapPath(reader["pic_url"].ToString()));
            }
            reader.Close();
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\DeviceRepair.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //往表中插入数据
                model.repair_class = model.repair_class.Trim();
                sheet1.GetRow(1).GetCell(1).SetCellValue(model.repair_class);
                sheet1.GetRow(2).GetCell(1).SetCellValue(device_name);
                sheet1.GetRow(3).GetCell(1).SetCellValue(model.repair_title);
                sheet1.GetRow(4).GetCell(1).SetCellValue(model.repair_nums);
                sheet1.GetRow(5).GetCell(1).SetCellValue(technology_name);
                sheet1.GetRow(6).GetCell(1).SetCellValue(model.repair_date.ToString("D"));
                sheet1.GetRow(7).GetCell(1).SetCellValue(model.repair_finsh.ToString("D"));
                sheet1.GetRow(8).GetCell(1).SetCellValue(model.repair_reasons);
                sheet1.GetRow(9).GetCell(1).SetCellValue(model.repair_conclusion);
                sheet1.GetRow(12).GetCell(1).SetCellValue(model.repair_consumption);
                sheet1.GetRow(14).GetCell(1).SetCellValue(model.repair_mark);
                //再往表格中插入前四张图片
                int index = 0;
                int temp = 1;
                int row = 10;
                int col = 1;
                while (index < pic_url.Count && temp <= 4)
                {
                    AddPieChart(sheet1, hssfworkbook, pic_url[index], row, col, ".png");
                    index += 1;
                    temp += 1;
                    if (temp == 2)
                    {
                        row = 10;
                        col = 2;
                    }
                    else if (temp == 3)
                    {
                        row = 11;
                        col = 1;
                    }
                    else if (temp == 4)
                    {
                        row = 11;
                        col = 2;
                    }

                }
                //AddPieChart(sheet1, hssfworkbook, sql, 1, 1,".png");
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, model.repair_title);
            }
            con.Close();
        }
        //导出老式的经理审批表
        public void OutputDeiveOldRepairExcel(string id)
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查询出对应的维修数据
            string sql = "select * from dm_device_repair where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            DeviceRepair model = new DeviceRepair();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.repair_date = Convert.ToDateTime(reader["repair_date"]);
                model.repair_finsh = Convert.ToDateTime(reader["repair_finsh"]);
                model.repair_class = reader["repair_class"].ToString();
                model.repair_title = reader["repair_title"].ToString();
                model.repair_nums = Convert.ToInt32(reader["repair_nums"]);
                model.repair_reasons = reader["repair_reasons"].ToString();
                model.repair_conclusion = reader["repair_conclusion"].ToString();
                model.repair_join = reader["repair_join"].ToString();
                model.repair_consumption = reader["repair_consumption"].ToString();
                model.repair_mark = reader["repair_mark"].ToString();
            }
            reader.Close();
            //查询出对应的设备名称
            sql = "select title from dm_device where id = " + model.device_id;
            cmd = new SqlCommand(sql, con);
            string device_name = cmd.ExecuteScalar().ToString();
            //查询出对应工艺段的名称
            sql = "select title from dm_technology where id = " + model.technology_id;
            cmd = new SqlCommand(sql, con);
            string technology_name = cmd.ExecuteScalar().ToString();
            //查询出对应的图片（最多四张）
            sql = "select pic_url from dm_device_repair_pic where type = 1 and repair_id = " + model.id;
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            List<string> pic_url = new List<string>();


            while (reader.Read())
            {
                pic_url.Add(Server.MapPath(reader["pic_url"].ToString()));
            }
            reader.Close();
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\DeviceRepairOldApproval.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //往表中插入数据
                model.repair_class = model.repair_class.Trim();
                sheet1.GetRow(1).GetCell(1).SetCellValue(model.repair_class);
                sheet1.GetRow(2).GetCell(1).SetCellValue(device_name);
                sheet1.GetRow(3).GetCell(1).SetCellValue(model.repair_title);
                sheet1.GetRow(4).GetCell(1).SetCellValue(model.repair_nums);
                sheet1.GetRow(5).GetCell(1).SetCellValue(technology_name);
                sheet1.GetRow(6).GetCell(1).SetCellValue(model.repair_date.ToString("D"));
                sheet1.GetRow(7).GetCell(1).SetCellValue(model.repair_finsh.ToString("D"));
                sheet1.GetRow(8).GetCell(1).SetCellValue(model.repair_reasons);
                sheet1.GetRow(9).GetCell(1).SetCellValue(model.repair_conclusion);
                sheet1.GetRow(13).GetCell(1).SetCellValue(model.repair_mark);
                //再往表格中插入前四张图片
                int index = 0;
                int temp = 1;
                int row = 10;
                int col = 1;
                while (index < pic_url.Count && temp <= 4)
                {
                    AddPieChart(sheet1, hssfworkbook, pic_url[index], row, col, ".png");
                    index += 1;
                    temp += 1;
                    if (temp == 2)
                    {
                        row = 10;
                        col = 2;
                    }
                    else if (temp == 3)
                    {
                        row = 11;
                        col = 1;
                    }
                    else if (temp == 4)
                    {
                        row = 11;
                        col = 2;
                    }

                }
                //AddPieChart(sheet1, hssfworkbook, sql, 1, 1,".png");
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, model.repair_title+"审批表");
            }
            con.Close();
        }
        //导出维修审批表（也就是要等经理预先审批后才行动的表）
        public void OutputDeivePreRepairExcel(string id)
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查询出对应的维修数据
            string sql = "select * from dm_device_repair where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            DeviceRepair model = new DeviceRepair();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.repair_begin = Convert.ToDateTime(reader["repair_begin"]);
                model.repair_starts = Convert.ToDateTime(reader["repair_starts"]);
                model.repair_class = reader["repair_class"].ToString();
                model.repair_title = reader["repair_title"].ToString();
                model.repair_nums = Convert.ToInt32(reader["repair_nums"]);
                model.repair_reasons = reader["repair_reasons"].ToString();
                model.repair_consume = double.Parse(reader["repair_consume"].ToString());

                model.repair_mark = reader["repair_mark"].ToString();
            }
            reader.Close();
            //查询出对应的设备名称
            sql = "select title from dm_device where id = " + model.device_id;
            cmd = new SqlCommand(sql, con);
            string device_name = cmd.ExecuteScalar().ToString();
            //查询出对应工艺段的名称
            sql = "select title from dm_technology where id = " + model.technology_id;
            cmd = new SqlCommand(sql, con);
            string technology_name = cmd.ExecuteScalar().ToString();
            //查询出对应的图片（最多四张）
            sql = "select pic_url from dm_device_repair_pic where type = 0 and repair_id = " + model.id;
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            List<string> pic_url = new List<string>();


            while (reader.Read())
            {
                pic_url.Add(Server.MapPath(reader["pic_url"].ToString()));
            }
            reader.Close();
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\DeviceRepairPreApproval.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //往表中插入数据
                model.repair_class = model.repair_class.Trim();
                sheet1.GetRow(1).GetCell(1).SetCellValue(model.repair_class);
                sheet1.GetRow(2).GetCell(1).SetCellValue(device_name);
                sheet1.GetRow(3).GetCell(1).SetCellValue(model.repair_title);
                sheet1.GetRow(4).GetCell(1).SetCellValue(model.repair_nums);
                sheet1.GetRow(5).GetCell(1).SetCellValue(technology_name);
                sheet1.GetRow(6).GetCell(1).SetCellValue(model.repair_begin.ToString("D"));
                sheet1.GetRow(7).GetCell(1).SetCellValue(model.repair_starts.ToString("D"));
                sheet1.GetRow(8).GetCell(1).SetCellValue(model.repair_consume);
                sheet1.GetRow(9).GetCell(1).SetCellValue(model.repair_reasons);
                sheet1.GetRow(13).GetCell(1).SetCellValue(model.repair_mark);
                //再往表格中插入前四张图片
                int index = 0;
                int temp = 1;
                int row = 10;
                int col = 1;
                while (index < pic_url.Count && temp <= 4)
                {
                    AddPieChart(sheet1, hssfworkbook, pic_url[index], row, col, ".png");
                    index += 1;
                    temp += 1;
                    if (temp == 2)
                    {
                        row = 10;
                        col = 2;
                    }
                    else if (temp == 3)
                    {
                        row = 11;
                        col = 1;
                    }
                    else if (temp == 4)
                    {
                        row = 11;
                        col = 2;
                    }

                }
                //AddPieChart(sheet1, hssfworkbook, sql, 1, 1,".png");
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, model.repair_title + "审批表");
            }
            con.Close();
        }
        //提前维修后审批的导出样式
        public void OutputDeiveAfterRepairExcel(string id)
        {
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查询出对应的维修数据
            string sql = "select * from dm_device_repair where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            DeviceRepair model = new DeviceRepair();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.technology_id = Convert.ToInt32(reader["technology_id"]);
                model.repair_date = Convert.ToDateTime(reader["repair_date"]);
                model.repair_finsh = Convert.ToDateTime(reader["repair_finsh"]);
                model.repair_class = reader["repair_class"].ToString();
                model.repair_title = reader["repair_title"].ToString();
                model.repair_nums = Convert.ToInt32(reader["repair_nums"]);
                model.repair_reasons = reader["repair_reasons"].ToString();
                model.repair_conclusion = reader["repair_conclusion"].ToString();
                model.repair_join = reader["repair_join"].ToString();
                model.repair_consumption = reader["repair_consumption"].ToString();
                model.repair_mark = reader["repair_mark"].ToString();
            }
            reader.Close();
            //查询出对应的设备名称
            sql = "select title from dm_device where id = " + model.device_id;
            cmd = new SqlCommand(sql, con);
            string device_name = cmd.ExecuteScalar().ToString();
            //查询出对应工艺段的名称
            sql = "select title from dm_technology where id = " + model.technology_id;
            cmd = new SqlCommand(sql, con);
            string technology_name = cmd.ExecuteScalar().ToString();
            //查询出对应的图片（查出所有区分故障和维修图片）
            sql = "select pic_url,describe,type from dm_device_repair_pic  where repair_id = " + model.id + "order by type;";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            //设置一个记录图片数量的变量
            int pic_num = 0;
            //设置一个关于图片的列表

            List<DeviceRepairPic> drp = new List<Models.DeviceRepairPic>();
            //建立一个对象用来复制用
            DeviceRepairPic temp_content ;

            while (reader.Read())
            {
                temp_content = new Models.DeviceRepairPic();
                temp_content.pic_url = (Server.MapPath(reader["pic_url"].ToString()));
                temp_content.describe = reader["describe"].ToString();
                temp_content.type = Convert.ToInt32(reader["type"]);
                drp.Add(temp_content);
                pic_num++;
            }
            reader.Close();
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\DeviceRepairAfterApproval.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("Sheet1");
                //往表中插入数据
                model.repair_class = model.repair_class.Trim();
                sheet1.GetRow(1).GetCell(1).SetCellValue(model.repair_class);
                sheet1.GetRow(2).GetCell(1).SetCellValue(device_name);
                sheet1.GetRow(3).GetCell(1).SetCellValue(model.repair_title);
                sheet1.GetRow(4).GetCell(1).SetCellValue(model.repair_nums);
                sheet1.GetRow(5).GetCell(1).SetCellValue(technology_name);
                sheet1.GetRow(6).GetCell(1).SetCellValue(model.repair_date.ToString("f"));
                sheet1.GetRow(7).GetCell(1).SetCellValue(model.repair_finsh.ToString("f"));
                sheet1.GetRow(8).GetCell(1).SetCellValue(model.repair_reasons);
                sheet1.GetRow(9).GetCell(1).SetCellValue(model.repair_conclusion);
                sheet1.GetRow(10).GetCell(1).SetCellValue(model.repair_consumption);
                sheet1.GetRow(12).GetCell(1).SetCellValue(model.repair_mark);
                //准备插入图片

                if (pic_num == 0)
                {
                    
                    sheet1.GetRow(13).GetCell(0).SetCellValue("此登记表无配图");
                }
                else
                {
                   
                    sheet1.GetRow(13).GetCell(0).SetCellValue("此表配图共" + pic_num + "张，详情请见图片附件");
                    //设置单元格样式
                    ICell Cell;
                    IRow Row;
                    ICellStyle cellstyle = hssfworkbook.CreateCellStyle();
                    //设置单元格上下左右边框线
                    cellstyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellstyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellstyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    cellstyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //开始遍历
                    int row = 29;
                    for (int i = 0; i < drp.Count; i++)
                    {
                        Cell = sheet1.CreateRow(row).CreateCell(0);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(row).GetCell(0).SetCellValue("类型");
                        Cell = sheet1.GetRow(row).CreateCell(1);
                        Cell.CellStyle = cellstyle;
                        if(drp[i].type==0)
                            sheet1.GetRow(row).GetCell(1).SetCellValue("故障/改造前图片");
                        else
                            sheet1.GetRow(row).GetCell(1).SetCellValue("维修/改造后图片");
                        row++;
                        Cell = sheet1.CreateRow(row).CreateCell(0);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(row).GetCell(0).SetCellValue("图片" + (i+1));
                        Cell = sheet1.GetRow(row).CreateCell(1);
                        Cell.CellStyle = cellstyle;
                        Row = sheet1.GetRow(row);
                        Row.Height = 20*355;
                        AddPieChart(sheet1, hssfworkbook, drp[i].pic_url, row, 1, ".png");
                        row++;
                        Cell = sheet1.CreateRow(row).CreateCell(0);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(row).GetCell(0).SetCellValue("描述");
                        Cell = sheet1.GetRow(row).CreateCell(1);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(row).GetCell(1).SetCellValue(drp[i].describe);
                        row++;

                    }                   
                }
                

                /*//再往表格中插入前四张图片
                int index = 0;
                int temp = 1;
                int row = 10;
                int col = 1;
                while (index < pic_url.Count && temp <= 4)
                {
                    AddPieChart(sheet1, hssfworkbook, pic_url[index], row, col, ".png");
                    index += 1;
                    temp += 1;
                    if (temp == 2)
                    {
                        row = 10;
                        col = 2;
                    }
                    else if (temp == 3)
                    {
                        row = 11;
                        col = 1;
                    }
                    else if (temp == 4)
                    {
                        row = 11;
                        col = 2;
                    }

                }*/
                //AddPieChart(sheet1, hssfworkbook, sql, 1, 1,".png");
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, model.repair_title+"维修审核登记表");
            }
            con.Close();
        }
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
        #region 向sheet插入图片
        /// <summary>
        /// 向sheet插入图片
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="workbook"></param>
        /// <param name="fileurl">文件路径</param>
        /// <param name="row">第几行</param>
        /// <param name="col">第几列</param>
        public static void AddPieChart(ISheet sheet, HSSFWorkbook workbook, string fileurl, int row, int col, string suffixType)
        {
            try
            {


                string FileName = fileurl;
                FileInfo file = new FileInfo(FileName);
                if (!string.IsNullOrEmpty(FileName) && file.Exists)
                {

                    byte[] bytes = System.IO.File.ReadAllBytes(FileName);
                    int pictureIdx = 0;
                    pictureIdx = workbook.AddPicture(bytes, PictureType.JPEG);
                    HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
                    HSSFClientAnchor anchor = new HSSFClientAnchor(10, 0, 2, 0, col, row, col + 1, row + 1);
                    //##处理照片位置，【图片左上角为（col, row）第row+1行col+1列，右下角为（ col +1, row +1）第 col +1+1行row +1+1列，第三个参数为宽，第四个参数为高

                    HSSFPicture pict = (HSSFPicture)patriarch.CreatePicture(anchor, pictureIdx);

                    // pict.Resize();//这句话一定不要，这是用图片原始大小来显示
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region 保存一个直接输出excel的方法
        /// <summary>
        /// 直接输出Excel
        /// </summary>
        /// <param name="dtData"></param>
        public static void DataTableToExcel(System.Data.DataTable dt, string excelName)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet("sheet1");
            IRow row = sheet.CreateRow(0);

            for (int i = 0; i < dt.Columns.Count; i++)
            {

                row.CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                IRow row2 = sheet.CreateRow(i + 1);



                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //获取后缀名
                    string fileName = dt.Rows[i][j].ToString();
                    string savefiletype = fileName.Substring(
                    fileName.LastIndexOf('.') + 1,
                    fileName.Length - fileName.LastIndexOf('.') - 1
                    ).ToLower(); //获取文件类型

                    if (savefiletype == "jpg" || savefiletype == "bmp"
                        || savefiletype == "jpeg" || savefiletype == "gif"
                        || savefiletype == "png")
                    {
                        row2.Height = 3000;
                        AddPieChart(sheet, book, dt.Rows[i][j].ToString(), i, j, savefiletype);
                        sheet.SetColumnWidth(j, 4000);

                    }
                    else
                    {
                        row2.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                    }
                }
            }

            //写入到客户端

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + HttpUtility.UrlEncode(excelName + ".xls", System.Text.Encoding.UTF8)));
            System.Web.HttpContext.Current.Response.BinaryWrite(ms.ToArray());
            book = null;
            ms.Close();
            ms.Dispose();
        }
        #endregion
        //导出维修清单用
        public void OutputDeviceRepairListModel(string data)
        {
            List<int> idd = new List<int>();
            string temp = "";
            foreach (char c in data)
            {
                if (c == 'n') break;
                if (c >= '0' && c <= '9')
                {
                    temp += c;
                }
                else
                {
                    idd.Add(Convert.ToInt32(temp));
                    temp = "";
                }


            }
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            SqlCommand cmd;
            SqlDataReader reader;
            string sql;
            int i = 1;
            int rows = 2;
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\RepairListModel.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("sheet1");
                //设置单元格样式
                ICellStyle cellstyle = hssfworkbook.CreateCellStyle();
                //设置单元格上下左右边框线
                cellstyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.WrapText = true;
                cellstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellstyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                //开始遍历查询
                ICell Cell;
                foreach (int item in idd)
                {
                    sql = "select * from dm_device_repair where id = " + item + ";";
                    cmd = new SqlCommand(sql, con);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        //往表中插入数据
                        //序号                       
                        Cell = sheet1.GetRow(rows).GetCell(0);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(0).SetCellValue(i);
                        //维修日期
                        Cell = sheet1.GetRow(rows).GetCell(1);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(1).SetCellValue(Convert.ToDateTime(reader["repair_date"]).ToString("yyyy-MM-dd"));
                        //设备
                        Cell = sheet1.GetRow(rows).GetCell(2);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(2).SetCellValue(reader["repair_title"].ToString());
                        //设备数量
                        Cell = sheet1.GetRow(rows).GetCell(3);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(3).SetCellValue(Convert.ToInt32(reader["repair_nums"]));
                        //原因
                        Cell = sheet1.GetRow(rows).GetCell(4);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(4).SetCellValue(reader["repair_reasons"].ToString());
                        //消耗
                        Cell = sheet1.GetRow(rows).GetCell(5);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(5).SetCellValue(reader["repair_consumption"].ToString());
                        //维修情况
                        Cell = sheet1.GetRow(rows).GetCell(6);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(6).SetCellValue(reader["repair_conclusion"].ToString());
                        //维修时间
                        Cell = sheet1.GetRow(rows).GetCell(7);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(7).SetCellValue(Convert.ToDateTime(reader["repair_finsh"]).ToString("yyyy-MM-dd"));
                        //备注
                        Cell = sheet1.GetRow(rows).GetCell(8);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(8).SetCellValue(reader["repair_mark"].ToString());
                        i += 1;
                    }
                    rows += 1;
                    reader.Close();
                }

                //下面是设置单元格边框的示例
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, "设备电气维修记录" + DateTime.Now.ToString("d"));
            }
            /*示例
            HSSFWorkbook workbook = new HSSFWorkbook();// 创建一个Excel文件   
            ISheet sheet = workbook.CreateSheet("Sheet1");
            ICellStyle cellstyle = workbook.CreateCellStyle();
            //设置单元格上下左右边框线
            cellstyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellstyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellstyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellstyle.WrapText = true;
            //把样式赋到对应的单元格上
            ICell Cell = sheet.CreateRow(1).CreateCell(1);
            Cell.CellStyle = cellstyle;
            Cell.SetCellValue("测试格式效果");
            //在前端进行下载
            MemoryStream mstream = new MemoryStream();
            workbook.Write(mstream);
            DownloadFile(mstream, "ceshi");
            */
        }


        /////////////////////////////////下面是设备保养的内容///////////////////////
        public ActionResult DeviceMaintenancePlan()
        {
            return View();
        }
        public string SelectDeviceMaintenancePlanList()
        {
            string str = "";
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();

            //先将对应的设备ID取出生成字典备用
            string sql = "select ID,title from dm_device";
            Dictionary<int, string> dic_device = new Dictionary<int, string>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_device.Add(Convert.ToInt32(reader["id"]), reader["title"].ToString());
            }
            reader.Close();
            //最后把设备维保养计划表给查询出来
            string month;
            if (Request["month"] == null) month = DateTime.Now.Month.ToString();
            else if (Request["month"] == "") month = "";
            else month = Request["month"].ToString();
            sql = "select * from dm_device_maintenance_plan where  Right(100+dmp_month,2) like '%" + month + "%' order by dmp_isfinish,dmp_weekend,order_id;";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            //设置一个变量count来记录数据条数
            int count = 0;
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                str += "{\"id\":\"" + reader["id"].ToString() + "\",";
                str += "\"device_id\":\"" + dic_device[Convert.ToInt32(reader["device_id"])] + "\",";
                str += "\"user_id\":\"" + reader["user_id"].ToString() + "\",";
                str += "\"order_id\":\"" + Convert.ToInt32(reader["order_id"]) + "\",";
                str += "\"dmp_content\":\"" + reader["dmp_content"].ToString() + "\",";
                str += "\"dmp_consumption\":\"" + reader["dmp_consumption"].ToString() + "\",";
                if (Convert.ToInt32(reader["dmp_isfinish"]) == 1)
                {
                    str += "\"dmp_isfinish\":\"" + "已完成" + "\",";
                }
                else
                {
                    str += "\"dmp_isfinish\":\"" + "未完成" + "\",";
                }
                str += "\"dmp_month\":\"" + Convert.ToInt32(reader["dmp_month"]) + "月\",";
                str += "\"dmp_weekend\":\"第" + Convert.ToInt32(reader["dmp_weekend"]) + "周\",";
                str += "\"remark\":\"" + reader["remark"].ToString() + "\"},";
                count += 1;
            }
            str = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + str;
            str = str + "]}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            reader.Close();
            con.Close();

            return json.ToString();
        }

        public ActionResult AddDeviceMaintenancePlan()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //设置月份下拉菜单
            var selectStatusList = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "1月" },
            new SelectListItem() { Value = "2", Text = "2月" },
            new SelectListItem() { Value = "3", Text = "3月" },
            new SelectListItem() { Value = "4", Text = "4月" },
            new SelectListItem() { Value = "5", Text = "5月" },
            new SelectListItem() { Value = "6", Text = "6月" },
            new SelectListItem() { Value = "7", Text = "7月" },
            new SelectListItem() { Value = "8", Text = "8月" },
            new SelectListItem() { Value = "9", Text = "9月" },
            new SelectListItem() { Value = "10", Text = "10月" },
            new SelectListItem() { Value = "11", Text = "11月" },
            new SelectListItem() { Value = "12", Text = "12月" },

        };
            ViewBag.month = selectStatusList;
            //设置周下拉菜单
            var selectStatusList2 = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "第1周" },
            new SelectListItem() { Value = "2", Text = "第2周" },
            new SelectListItem() { Value = "3", Text = "第3周" },
            new SelectListItem() { Value = "4", Text = "第4周" },
            new SelectListItem() { Value = "5", Text = "第5周" },

        };
            ViewBag.weekend = selectStatusList2;
            con.Close();
            return View();
        }
        public JavaScriptResult AddDeviceMaintenancePlan_post(DeviceMaintenancePlan model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "select  Max(order_id) from dm_device_maintenance_plan where dmp_month = '" + model.dmp_month + "' and dmp_weekend='" + model.dmp_weekend + "';";
            SqlCommand cmd = new SqlCommand(sql, con);
            //设置order_id排序变量
            int order_id;
            if (cmd.ExecuteScalar() == System.DBNull.Value)
            {
                order_id = 1;
            }
            else
            {
                order_id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            //正式插入数据
            sql = "insert into dm_device_maintenance_plan(device_id,user_id,order_id,dmp_content,dmp_consumption,dmp_isfinish,dmp_month,dmp_weekend,remark) values(" + model.device_id + "," + Convert.ToInt32(Session["user_id"]) + "," + order_id + ",'" + model.dmp_content + "','" + model.dmp_consumption + "'," + 0 + "," + model.dmp_month + "," + model.dmp_weekend + ",'" + model.remark + "');";
            cmd = new SqlCommand(sql, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check == 1)
                return JavaScript("swal_success();jump_DeviceMaintenancePlan();");
            else
                return JavaScript("swal_error();");
        }
        //将保养计划添加至保养清单中去
        public string DeviceMaintenancePlanFinish(string id, string date)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //查出设备保养计划表的内容
            string sql = "select * from dm_device_maintenance_plan where id = " + id;
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            //定义一个保养计划类
            DeviceMaintenancePlan dmp = new DeviceMaintenancePlan();
            while (reader.Read())
            {
                dmp.device_id = Convert.ToInt32(reader["device_id"]);
                dmp.dmp_content = reader["dmp_content"].ToString();
                dmp.dmp_consumption = reader["dmp_consumption"].ToString();
                dmp.dmp_month = Convert.ToInt32(reader["dmp_month"]);
                dmp.dmp_weekend = Convert.ToInt32(reader["dmp_weekend"]);
                dmp.remark = reader["remark"].ToString();
            }
            reader.Close();
            //将数据插入到保养清单中去
            sql = "insert into dm_device_maintenance values(" + dmp.device_id + "," + Convert.ToInt32(Session["user_id"]) + ",'" + dmp.dmp_content + "','" + dmp.dmp_consumption + "'," + 1 + ",'" + date + "'," + dmp.dmp_weekend + "," + 0 + ",'" + dmp.remark + "');";
            //更新保养计划的isfinish列
            sql += "update dm_device_maintenance_plan set dmp_isfinish = " + 1 + " where id = " + id + ";";
            cmd = new SqlCommand(sql, con);
            int check = Convert.ToInt32(cmd.ExecuteNonQuery());
            con.Close();
            if (check == 2) return "已完成该项保养！！";
            else return "保养添加失败，请联系管理员解决！！";
        }

        //修改保养计划内容
        public ActionResult ModifyDeviceMaintenancePlan(string id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //先查出对应的设备来
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //设置月份下拉菜单
            var selectStatusList = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "1月" },
            new SelectListItem() { Value = "2", Text = "2月" },
            new SelectListItem() { Value = "3", Text = "3月" },
            new SelectListItem() { Value = "4", Text = "4月" },
            new SelectListItem() { Value = "5", Text = "5月" },
            new SelectListItem() { Value = "6", Text = "6月" },
            new SelectListItem() { Value = "7", Text = "7月" },
            new SelectListItem() { Value = "8", Text = "8月" },
            new SelectListItem() { Value = "9", Text = "9月" },
            new SelectListItem() { Value = "10", Text = "10月" },
            new SelectListItem() { Value = "11", Text = "11月" },
            new SelectListItem() { Value = "12", Text = "12月" },

        };
            ViewBag.month = selectStatusList;
            //设置周下拉菜单
            var selectStatusList2 = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "第1周" },
            new SelectListItem() { Value = "2", Text = "第2周" },
            new SelectListItem() { Value = "3", Text = "第3周" },
            new SelectListItem() { Value = "4", Text = "第4周" },
            new SelectListItem() { Value = "5", Text = "第5周" },

        };
            ViewBag.weekend = selectStatusList2;
            //开始干正事
            string sql = "select * from dm_device_maintenance_plan where id = " + id + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            DeviceMaintenancePlan model = new Models.DeviceMaintenancePlan();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(id);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.dmp_content = reader["dmp_content"].ToString();
                model.dmp_consumption = reader["dmp_consumption"].ToString();
                model.dmp_month = Convert.ToInt32(reader["dmp_month"]);
                model.dmp_weekend = Convert.ToInt32(reader["dmp_weekend"]);
                model.remark = reader["remark"].ToString();
            }
            reader.Close();
            con.Close();
            return View(model);
        }
        //修改保养计划提交
        public JavaScriptResult ModifyDeviceMaintenancePlan_Post(DeviceMaintenancePlan model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "update dm_device_maintenance_plan set device_id =" + model.device_id +
                ",user_id = " + Convert.ToInt32(Session["user_id"]) +
                ",dmp_content = '" + model.dmp_content +
                "',dmp_consumption = '" + model.dmp_consumption +
                "',dmp_isfinish = " + 0 +
                ",dmp_month = " + model.dmp_month +
                ",dmp_weekend = " + model.dmp_weekend +
                ",remark = '" + model.remark + "' where id = " + Request.Form["modify"] + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            //这里还需要加一条，就是给这条更新数据所在的月和周的所有数据重新赋予新的排序编号
            //1）先查出新的该月该周的所有数据id
            List<int> temp_id = new List<int>();
            sql = "select id from dm_device_maintenance_plan where dmp_month = " + model.dmp_month + " and dmp_weekend = " + model.dmp_weekend + ";";
            cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                temp_id.Add(Convert.ToInt32(reader["id"]));
            }
            reader.Close();
            //2)for循环给每个order_id重新编号
            int temp;
            for(int j = 0; j < temp_id.Count; j++)
            {
                sql = "update dm_device_maintenance_plan set order_id = " + j + " where id = " + temp_id[j] + ";";
                cmd = new SqlCommand(sql, con);
                temp = cmd.ExecuteNonQuery();
            }
            con.Close();
            if (check == 1)
            {
                return JavaScript("jump_DeviceMaintenancePlan();");
            }
            else
            {
                return JavaScript("swal_error();");
            }

        }
        //删除保养计划
        public string DeleteDeviceMaintenancePlan(string id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "delete from dm_device_maintenance_plan where id = " + id + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1)
            {
                return "";
            }
            else
            {
                return "";
            }

        }
        //初始化保养计划
        public string DeviceMaintenancePlanInitialize(string data)
        {
            List<int> idd = new List<int>();
            string temp = "";
            foreach (char c in data)
            {
                if (c == 'n') break;
                if (c >= '0' && c <= '9')
                {
                    temp += c;
                }
                else
                {
                    idd.Add(Convert.ToInt32(temp));
                    temp = "";
                }


            }
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql;
            SqlCommand cmd;
            int check = 0;
            foreach (int item in idd)
            {
                sql = "update dm_device_maintenance_plan set dmp_isfinish = 0 where id = " + item + ";";
                cmd = new SqlCommand(sql, con);
                check += cmd.ExecuteNonQuery();
            }
            con.Close();
            if (check > 0) return "初始化成功！";
            else return "初始化失败！";
        }
        //保养清单
        public ActionResult DeviceMaintenance()
        {
            return View();
        }
        public string DeviceMaintenanceList()
        {
            string str = "";
            string sql;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            SqlCommand cmd;
            SqlDataReader reader;
            //最后把设备维保养表给查询出来
            string month;
            if (Request["month"] == null) month = DateTime.Now.Month.ToString();
            else if (Request["month"] == "") month = "";
            else month = Request["month"].ToString();
            string year;
            if (Request["year"] == null) year = DateTime.Now.Year.ToString();
            else if (Request["year"] == "") year = "";
            else year = Request["year"].ToString();
            string keyword;
            if (Request["keyword"] == null) keyword = "";
            else if (Request["keyword"] == "") keyword = "";
            else keyword = Request["keyword"].ToString();
            sql = "select dm_device_maintenance.id,dm_device.title,dm_content,dm_consumption,dm_isfinish,dm_date,dm_weekend,dm_isextra,remark from dm_device_maintenance,dm_device where dm_device.id = device_id and Right(100+Month(dm_date),2) like '%" + month + "%' and dm_device.title like '%" + keyword + "%' and CONVERT(VARCHAR,Year(dm_date)) like '%" + year + "%' order by dm_weekend,dm_date desc;";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            //设置一个变量count来记录数据条数
            int count = 0;
            while (reader.Read())    // 判断数据是否读到尾. 
            {
                str += "{\"id\":\"" + reader["id"].ToString() + "\",";
                str += "\"device_id\":\"" + reader["title"].ToString() + "\",";
                str += "\"dm_content\":\"" + reader["dm_content"].ToString() + "\",";
                str += "\"dm_consumption\":\"" + reader["dm_consumption"].ToString() + "\",";
                if (Convert.ToInt32(reader["dm_isfinish"]) == 1)
                {
                    str += "\"dm_isfinish\":\"" + "已完成" + "\",";
                }
                else
                {
                    str += "\"dm_isfinish\":\"" + "未完成" + "\",";
                }

                str += "\"dm_date\":\"" + Convert.ToDateTime(reader["dm_date"]).ToString("yyyy-MM-dd") + "\",";
                str += "\"dm_weekend\":\"第" + Convert.ToInt32(reader["dm_weekend"]) + "周\",";
                if (Convert.ToInt32(reader["dm_isextra"]) == 0)
                {
                    str += "\"dm_isextra\":\"" + "计划内" + "\",";
                }
                else
                {
                    str += "\"dm_isextra\":\"" + "计划外" + "\",";
                }
                str += "\"remark\":\"" + reader["remark"].ToString() + "\"},";
                count += 1;
            }
            str = "{\"code\": 0,\"count\":" + count + ",\"data\": [" + str;
            str = str + "]}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            reader.Close();
            con.Close();

            return json.ToString();
        }
        //额外保养的添加
        public ActionResult AddDeviceMaintenance()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //设置周下拉菜单
            var selectStatusList2 = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "第1周" },
            new SelectListItem() { Value = "2", Text = "第2周" },
            new SelectListItem() { Value = "3", Text = "第3周" },
            new SelectListItem() { Value = "4", Text = "第4周" },
            new SelectListItem() { Value = "5", Text = "第5周" },

            };
            ViewBag.weekend = selectStatusList2;
            con.Close();

            return View();
        }
        public JavaScriptResult AddDeviceMaintenance_Post(DeviceMaintenance model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "insert into dm_device_maintenance(device_id,user_id,dm_content,dm_consumption,dm_isfinish,dm_date,dm_weekend,dm_isextra,remark) values(" + model.device_id + "," + Convert.ToInt32(Session["user_id"]) + ",'" + model.dm_content + "','" + model.dm_consumption + "'," + 1 + ",'" + model.dm_date + "'," + model.dm_weekend + "," + 1 + ",'" + model.remark + "');";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1) return JavaScript("swal_success();jump_DeviceMaintenance();");
            else return JavaScript("swal_error();");
        }
        //修改保养内容
        public ActionResult ModifyDeviceMaintenance(string id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            //1)查出所有的设备名称以及对应的工艺段
            string sqlStr = "select dm_device.ID,dm_device.title,dm_technology.title from dm_device,dm_technology where technology_id = dm_technology.id order by technology_id; ";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SelectListItem> list_device = new List<SelectListItem>();
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString() + " 丨 " + ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //设置周下拉菜单
            var selectStatusList2 = new List<SelectListItem>() {
            new SelectListItem() { Value = "1", Text = "第1周" },
            new SelectListItem() { Value = "2", Text = "第2周" },
            new SelectListItem() { Value = "3", Text = "第3周" },
            new SelectListItem() { Value = "4", Text = "第4周" },
            new SelectListItem() { Value = "5", Text = "第5周" },

            };
            ViewBag.weekend = selectStatusList2;
            //查出对应设备保养内容
            string sql = "select * from dm_device_maintenance where id = " + id + ";";
            DeviceMaintenance model = new Models.DeviceMaintenance();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.id = Convert.ToInt32(reader["id"]);
                model.device_id = Convert.ToInt32(reader["device_id"]);
                model.dm_content = reader["dm_content"].ToString();
                model.dm_consumption = reader["dm_consumption"].ToString();
                model.dm_date = Convert.ToDateTime(Convert.ToDateTime(reader["dm_date"]).ToString("yyyy-MM-dd"));
                model.dm_weekend = Convert.ToInt32(reader["dm_weekend"]);
                model.remark = reader["remark"].ToString();
            }
            con.Close();
            return View(model);
        }
        public JavaScriptResult ModifyDeviceMaintenance_Post(DeviceMaintenance model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "update dm_device_maintenance set device_id = " + model.device_id +
                ",user_id = " + Convert.ToInt32(Session["user_id"]) +
                ",dm_content = '" + model.dm_content +
                "',dm_consumption = '" + model.dm_consumption +
                "',dm_date = '" + model.dm_date +
                "',dm_weekend = " + model.dm_weekend +
                ",remark = '" + model.remark + "' where id = " + Request.Form["idd"].ToString() + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1) return JavaScript("swal_success();jump_DeviceMaintenance();");
            else return JavaScript("swal_error();");
        }
        //删除保养记录
        public string DeleteDeviceMaintenance(string id)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "delete from dm_device_maintenance where id=" + id + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 1) return "";
            else return "";
        }
        //调换设备保养计划同周期的两列数据的order_id
        public string ResortDeviceMaintenancePlan(string data, string order)
        {
            //获取对应的两个ID数据
            List<int> idd = new List<int>();
            string temp = "";
            foreach (char c in data)
            {
                if (c == 'n') break;
                if (c >= '0' && c <= '9')
                {
                    temp += c;
                }
                else
                {
                    idd.Add(Convert.ToInt32(temp));
                    temp = "";
                }


            }
            //获取对应的两个order_id数据
            List<int> order_id = new List<int>();
            temp = "";
            foreach (char c in order)
            {
                if (c == 'n') break;
                if (c >= '0' && c <= '9')
                {
                    temp += c;
                }
                else
                {
                    order_id.Add(Convert.ToInt32(temp));
                    temp = "";
                }

            }
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sql = "update dm_device_maintenance_plan set order_id = " + order_id[1] + " where id = " + idd[0] + ";";
            sql += "update dm_device_maintenance_plan set order_id = " + order_id[0] + " where id = " + idd[1] + ";";
            SqlCommand cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check == 2) return "调换顺序成功！";
            else return "调换顺序出错，请及时联系管理员解决~！";

        }
        //保养内容excel导出
        public void OutputDeviceMaintenance(string data)
        {
            List<int> idd = new List<int>();
            string temp = "";
            foreach (char c in data)
            {
                if (c == 'n') break;
                if (c >= '0' && c <= '9')
                {
                    temp += c;
                }
                else
                {
                    idd.Add(Convert.ToInt32(temp));
                    temp = "";
                }


            }
            //连接数据库
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            SqlCommand cmd;
            SqlDataReader reader;
            string sql;
            int i = 1;
            int rows = 2;
            int weekend = 0;//用来分别周期
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(HttpContext.Request.PhysicalApplicationPath + @"ExcelModel\MaintenanceListModel.xls", FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
                ISheet sheet1 = hssfworkbook.GetSheet("sheet1");
                //设置单元格样式
                ICellStyle cellstyle = hssfworkbook.CreateCellStyle();
                //设置单元格上下左右边框线
                cellstyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                cellstyle.WrapText = true;
                cellstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellstyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                //开始遍历查询
                ICell Cell;
                
                foreach (int item in idd)
                {
                    sql = "select dm_device_maintenance.id,dm_device.title,dm_content,dm_consumption,dm_date,dm_weekend,dm_isextra,remark from dm_device,dm_device_maintenance where dm_device.id=dm_device_maintenance.device_id and dm_device_maintenance.id =  " + item + ";";
                    cmd = new SqlCommand(sql, con);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        //如果周期不符则合并一行单元格显示第几周
                        if(Convert.ToInt32(reader["dm_weekend"]) != weekend)
                        {
                            sheet1.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rows, rows, 0, 7));
                           for(int j = 0; j < 8; j++)
                            {
                                Cell = sheet1.GetRow(rows).GetCell(j);
                                Cell.CellStyle = cellstyle;
                            }
                            //写入第几周的数据
                            sheet1.GetRow(rows).GetCell(0).SetCellValue("第" + reader["dm_weekend"].ToString() + "周");
                            weekend = Convert.ToInt32(reader["dm_weekend"]);
                            rows += 1;
                        }
                        //往表中插入数据
                        //序号                       
                        Cell = sheet1.GetRow(rows).GetCell(0);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(0).SetCellValue(i);
                        //保养设备
                        Cell = sheet1.GetRow(rows).GetCell(1);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(1).SetCellValue(reader["title"].ToString());
                        //保养项目
                        Cell = sheet1.GetRow(rows).GetCell(2);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(2).SetCellValue(reader["dm_content"].ToString());
                        //物品使用情况
                        Cell = sheet1.GetRow(rows).GetCell(3);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(3).SetCellValue(reader["dm_consumption"].ToString());
                        //保养进度
                        Cell = sheet1.GetRow(rows).GetCell(4);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(4).SetCellValue("已完成");
                        //保养日期
                        Cell = sheet1.GetRow(rows).GetCell(5);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(5).SetCellValue(Convert.ToDateTime(reader["dm_date"]).ToString("yyyy-MM-dd"));
                        //是否按计划执行
                        Cell = sheet1.GetRow(rows).GetCell(6);
                        Cell.CellStyle = cellstyle;
                        if(Convert.ToInt32(reader["dm_isextra"])==0)
                            sheet1.GetRow(rows).GetCell(6).SetCellValue("计划内");
                        else
                            sheet1.GetRow(rows).GetCell(6).SetCellValue("计划外");
                        //备注
                        Cell = sheet1.GetRow(rows).GetCell(7);
                        Cell.CellStyle = cellstyle;
                        sheet1.GetRow(rows).GetCell(7).SetCellValue(reader["remark"].ToString());
                        i += 1;
                    }
                    rows += 1;
                    reader.Close();
                }

                //下面是设置单元格边框的示例
                MemoryStream mstream = new MemoryStream();
                hssfworkbook.Write(mstream);
                DownloadFile(mstream, "设备保养记录" + DateTime.Now.ToString("d"));
            }
        }
        //导入设备保养计划
        public string InputDeviceMaintenancePlan()
        {
            HttpPostedFileBase ff = Request.Files["file"];
            //获得保存文件
             string filePath = Path.Combine(HttpContext.Server.MapPath("../ExcelModel/Upload"),
            Path.GetFileName(ff.FileName));
            ff.SaveAs(filePath);
            string name = ff.FileName;
            using (FileStream file = new FileStream(HttpContext.Server.MapPath("../ExcelModel/Upload/") + ff.FileName, FileMode.Open, FileAccess.Read))
            {
                HSSFWorkbook hssfworkbook = new HSSFWorkbook(file);
                //删除原有文件
                //删除对应的实体文件
                string filePath2 = HttpContext.Server.MapPath("../ExcelModel/Upload/") + ff.FileName;//路径 
                FileInfo file2 = new FileInfo(filePath);
                if (file2.Exists)
                {
                    file2.Delete();
                }
                ISheet sheet1 = hssfworkbook.GetSheet("sheet1");
                name = sheet1.GetRow(0).GetCell(0).ToString();
            }
                /*string str = "{\"code\": 0,\"count\":" + 1 + ",\"data\": [" + "";
                str = str + "]}";*/
                string str = "{\"code\": \""+name+"\"}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
         
            return json.ToString();
        }
    }
}