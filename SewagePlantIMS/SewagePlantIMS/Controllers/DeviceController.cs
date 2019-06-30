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
            string str="";
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            
            //先将对应的设备ID取出生成字典备用
            string sql = "select ID,title from dm_device";
            Dictionary<int, string> dic_device = new Dictionary<int, string>();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dic_device.Add(Convert.ToInt32(reader["id"]),reader["title"].ToString());
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
            sql = "select id,device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark from dm_device_repair order by repair_date desc;";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
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
            }
            str = "{\"code\": 0,\"data\": [" + str;
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
                list_device.Add(new SelectListItem() { Value = "" + ds.Tables[0].Rows[mDr][0].ToString() + "", Text = "" + ds.Tables[0].Rows[mDr][2].ToString()+" 丨 " +ds.Tables[0].Rows[mDr][1].ToString() + "" });
            }
            ViewBag.list_device = list_device;
            //2)设置一下维修类别
            List<SelectListItem> state = new List<SelectListItem>();
            state.Add(new SelectListItem() { Value = "常规维护", Text = "常规维护" });
            state.Add(new SelectListItem() { Value = "故障检修", Text = "故障检修" });
            state.Add(new SelectListItem() { Value = "突发维修", Text = "突发维修" });
            state.Add(new SelectListItem() { Value = "其他", Text = "其他" });
            ViewBag.state = state;
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
            sql = "insert into dm_device_repair(device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark) values(" +
                    model.device_id + "," + Session["user_id"] + "," + dic_id[model.device_id] + ",'" + model.repair_date + "','" + model.repair_finsh + "','" + model.repair_class + "','" + model.repair_title + "'," + model.repair_nums + ",'" + model.repair_reasons + "','" + model.repair_conclusion + "','" + model.repair_join + "','" + model.repair_consumption + "','" + model.repair_mark + "')";
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
            state.Add(new SelectListItem() { Value = "其他", Text = "其他" });
            ViewBag.state = state;
            //3)查出对应ID的设备维修详细内容
            sqlStr = "select id,device_id,user_id,technology_id,repair_date,repair_finsh,repair_class,repair_title,repair_nums,repair_reasons,repair_conclusion,repair_join,repair_consumption,repair_mark from dm_device_repair where id = " + id + ";";
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
                  "update dm_device_repair set repair_mark = '" + model.repair_mark + "' where id = " + Request.Form["idd"] + ";";
            cmd = new SqlCommand(sql, con);
            int check = cmd.ExecuteNonQuery();
            con.Close();
            if (check != 0)
                return JavaScript("swal_success();jump_DeviceRepair();");
            else
                return JavaScript("swal_error();");
        }
        public ActionResult DeviceRepairPic(string id)
        {
            return View();
        }
        public string DeviceRepairPic_Post()
        {

            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            //获取文件名后缀
            string extName = Path.GetExtension(file.FileName).ToLower();
            //获取保存目录的物理路径
            if (System.IO.Directory.Exists(Server.MapPath("images/DeviceRepairPic/")) == false)//如果不存在就创建images文件夹
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("images/DeviceRepairPic/"));
            }
            string path = Server.MapPath("images/DeviceRepairPic/"); //path为某个文件夹的绝对路径，不要直接保存到数据库
                                                      //    string path = "F:\\TgeoSmart\\Image\\";
                                                      //生成新文件的名称，guid保证某一时刻内图片名唯一（文件不会被覆盖）
            string fileNewName = Guid.NewGuid().ToString();
            string ImageUrl = path + fileNewName + extName;
            //SaveAs将文件保存到指定文件夹中
            file.SaveAs(ImageUrl);
            string str = "\"src\": \"http://cdn.layui.com/123.jpg\"";
            str = "{\"code\": 0,\"data\": {" + str;
            str = str + "}}";
            JObject json = (JObject)JsonConvert.DeserializeObject(str.ToString());
            return json.ToString();
        }
    }
}