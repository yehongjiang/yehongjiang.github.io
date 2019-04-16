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



    }
}