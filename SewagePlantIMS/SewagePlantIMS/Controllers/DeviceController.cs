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
    //[LoginAttribute(isNeed = true)]
    public class DeviceController : Controller
    {
        // GET: Device
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
            con.Close();
            List<string> technology_name = new List<string>();
            List<string> class_name = new List<string>();
            List<string> brand_name = new List<string>();
            foreach (var data in model)
            {
                //data.
            }
            return View(model);
        }
    }
}