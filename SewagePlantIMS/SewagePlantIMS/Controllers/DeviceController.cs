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
    }
}