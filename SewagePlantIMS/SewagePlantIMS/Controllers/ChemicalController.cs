using SewagePlantIMS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SewagePlantIMS.Controllers
{
    public class ChemicalController : Controller
    {
        // GET: Chemical
        public ActionResult ChemicalDeviceList()
        {
            List<chemical_device> model = new List<chemical_device>();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SewagePlantIMS"].ConnectionString);
            con.Open();
            string sqlStr = "select id,cd_num,cd_name,cd_preserver from dm_chemical_device;";
            SqlDataAdapter da = new SqlDataAdapter(sqlStr, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            chemical_device[] E = new chemical_device[ds.Tables[0].Rows.Count];
            for (int mDr = 0; mDr < ds.Tables[0].Rows.Count; mDr++)
            {
                E[mDr] = new chemical_device();
                E[mDr].id = Convert.ToInt32(ds.Tables[0].Rows[mDr][0]);
                E[mDr].cd_num = ds.Tables[0].Rows[mDr][1].ToString();
                E[mDr].cd_name = ds.Tables[0].Rows[mDr][2].ToString();
                E[mDr].cd_preserver = ds.Tables[0].Rows[mDr][3].ToString();
                model.Add(E[mDr]);

                //直接生成二维码图片
                // CreateQrCode(E[mDr].id, E[mDr].elec_name);
            }
            con.Close();
            return View(model);
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
    }
}