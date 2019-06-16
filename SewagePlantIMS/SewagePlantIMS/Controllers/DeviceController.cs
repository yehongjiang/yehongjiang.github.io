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
                if(reader["purchase_date"] != DBNull.Value) model.purchase_date = Convert.ToDateTime(reader["purchase_date"]);   //特殊情况
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
            state.Add(new SelectListItem() { Value = "0", Text = "异常"});
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
            string sql = "select id,title,class_id,technology_id,state,brand_id,device_power,device_model,summary,purchase_date from dm_device where id = " + id +";";
            SqlCommand cmd = new SqlCommand(sql,con);
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
            sql = "select id,pic_url from dm_device_pic where device_id = " + model.id +";";
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
            string sqlStr = "select title from dm_device_class where id = " + model.class_id +";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_class = cmd.ExecuteScalar().ToString() ;
            //2)查出所有工艺段
            sqlStr = "select title from dm_technology where id = " + model.technology_id + ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_tecnology = cmd.ExecuteScalar().ToString() ;
            //3)查出所有的品牌
            sqlStr = "select title from dm_supplier_brand where id = " + model.brand_id +  ";";
            cmd = new SqlCommand(sqlStr, con);
            ViewBag.list_brand = cmd.ExecuteScalar().ToString();
            //4)设置一下状态
            if(model.state == 1)
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
            
            /*string js = "";
            // 创建一个 StreamReader 的实例来读取文件 
            // using 语句也能关闭 StreamReader
            string path = "data222.txt";
            using (StreamReader sr = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(path), System.Text.Encoding.Default))
            {
                string line;
                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    js += line;
                }
            }
            ViewBag.data = js;*/
            return View();
        }
        //将sql查询的内容转化为JSON格式
        public ActionResult SelectDeviceRepairList()
        {
            string js = "";
            // 创建一个 StreamReader 的实例来读取文件 
            // using 语句也能关闭 StreamReader
            using (StreamReader sr = new StreamReader("‪C:/Users/11619/Desktop/data222.txt"))
            {
                string line;
                ;
                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    js += line;
                }
            }
       
            return Content(js);
        }
    }
}