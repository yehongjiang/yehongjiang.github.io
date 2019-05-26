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
            string sqlStr = "select id,pic_url from dm_device_pic where id = " + ViewBag.id + "; ";
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
    }
}